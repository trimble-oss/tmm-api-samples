using System.Diagnostics;
using System.Net.Http.Headers;
using MauiSample.AccessCode;
using MauiSample.Models;
using Newtonsoft.Json.Linq;

namespace MauiSample;

internal static class RestApiService
{
  private static readonly Lazy<HttpClient> _lazyClient = new(() =>
  {
    var baseAddress = $"http://localhost:{PortInfo.ApiPort}/";

    return new HttpClient
    {
      BaseAddress = new Uri(baseAddress),
      Timeout = TimeSpan.FromSeconds(60)
    };
  });

  private static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
  private static HttpClient Client => _lazyClient.Value;

  public static AccessCodeVersion AccessCodeVersion { get; set; } = AccessCodeVersion.V2;

  public static async Task<string?> GetPublicKeyAsync()
  {
    using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    using HttpResponseMessage? response = await Client.GetAsync("api/v1/publicKey", cts.Token).ConfigureAwait(false);
    if (response is null || response.IsSuccessStatusCode == false)
    {
      Debug.WriteLine($"[GetPublicKeyAsync] Failed to get public key. Status: {response?.StatusCode}");
      return null;
    }
    string jwk = await response.Content.ReadAsStringAsync();
    Debug.WriteLine($"public key: {jwk}");
    return jwk;
  }

  public static async Task<ReceiverInfo?> GetReceiverAsync()
  {
    SetAuthorizationHeader();
    using CancellationTokenSource cts = new CancellationTokenSource(DefaultTimeout);
    using HttpResponseMessage? response = await Client.GetAsync("api/v1/receiver", cts.Token).ConfigureAwait(false);

    if (response is null || response.IsSuccessStatusCode == false)
    {
      Debug.WriteLine($"[GetReceiverAsync] Failed to get receiver. Status: {response?.StatusCode}");
      return null;
    }

    string payload = await response.Content.ReadAsStringAsync();
    return JToken.Parse(payload).ToObject<ReceiverInfo>();
  }

  public static async Task PutReceiverAsync(bool isConnected)
  {
    SetAuthorizationHeader();
    var payload = new JObject
    {
      ["isConnected"] = isConnected
    };
    using StringContent content = new(payload.ToString(), System.Text.Encoding.UTF8, "application/json");
    using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    using HttpResponseMessage? response = await Client.PutAsync("api/v1/receiver", content, cts.Token).ConfigureAwait(false);
    if (response is null || response.IsSuccessStatusCode == false)
    {
      Debug.WriteLine($"[PutReceiverAsync] Failed to update receiver. Status: {response?.StatusCode}");
    }
  }

  private static void SetAuthorizationHeader()
  {
    if (!Guid.TryParse(Values.AppID, out Guid appID))
    {
      throw new InvalidOperationException($"Invalid App ID \"{Values.AppID}\"");
    }

    string scheme;
    string accessCode;
    switch (AccessCodeVersion)
    {
      case AccessCodeVersion.V1:
        scheme = "Basic";
        accessCode = AccessCodeV1.Generate(appID, DateTime.UtcNow);
        break;
      case AccessCodeVersion.V2:
        scheme = "AccessCodeV2";
        accessCode = AccessCodeV2.Generate(appID, DateTime.UtcNow);
        break;
      default:
        throw new InvalidOperationException($"Unsupported Access Code Version {AccessCodeVersion}");
    }

    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, accessCode);
  }
}
