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
      Timeout = DefaultTimeout
    };
  });

  private static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
  private static HttpClient Client => _lazyClient.Value;

  public static AccessCodeVersion AccessCodeVersion { get; set; } = AccessCodeVersion.V1;

  public static async Task<string?> GetPublicKeyAsync()
  {
    using HttpResponseMessage? response = await Client.GetAsync("api/v1/publicKey").ConfigureAwait(false);
    if (response is null || response.IsSuccessStatusCode == false)
    {
      Debug.WriteLine($"[GetPublicKeyAsync] Failed to get public key. Status: {response?.StatusCode}");
      return null;
    }
    string jwk = await response.Content.ReadAsStringAsync();
    AccessCodeV2.SetPublicKey(jwk);
    return jwk;
  }

  public static async Task<ReceiverInfo?> GetReceiverAsync()
  {
    SetAuthorizationHeader();
    using HttpResponseMessage? response = await Client.GetAsync("api/v1/receiver").ConfigureAwait(false);

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
    try
    {
      SetAuthorizationHeader();
      Client.Timeout = TimeSpan.FromSeconds(30);
      var payload = new JObject
      {
        ["isConnected"] = isConnected
      };
      using StringContent content = new(payload.ToString(), System.Text.Encoding.UTF8, "application/json");
      using HttpResponseMessage? response = await Client.PutAsync("api/v1/receiver", content).ConfigureAwait(false);
      if (response is null || response.IsSuccessStatusCode == false)
      {
        Debug.WriteLine($"[PutReceiverAsync] Failed to update receiver. Status: {response?.StatusCode}");
      }
    }
    finally
    {
      Client.Timeout = DefaultTimeout;
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
