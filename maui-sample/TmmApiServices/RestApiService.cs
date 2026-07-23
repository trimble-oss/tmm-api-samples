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
    var baseAddress = $"http://localhost:{PortInfo.APIPort}/";

    return new HttpClient
    {
      BaseAddress = new Uri(baseAddress),
      Timeout = DefaultTimeout
    };
  });

  private static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

  private static HttpClient Client => _lazyClient.Value;

  public static async Task<string?> GetPublicKeyAsync()
  {
    using HttpResponseMessage? response = await Client.GetAsync("api/v1/publicKey").ConfigureAwait(false);
    if (response is null || response.IsSuccessStatusCode == false)
    {
      Debug.WriteLine($"[GetPublicKeyAsync] Failed to get public key. Status: {response?.StatusCode}");
      return null;
    }
    return await response.Content.ReadAsStringAsync();
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
    string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);
  }
}
