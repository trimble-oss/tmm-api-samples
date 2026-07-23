using System.Diagnostics;
using System.Net.Http.Headers;
using MauiSample.AccessCode;
using MauiSample.Models;
using Newtonsoft.Json.Linq;

namespace MauiSample
{
  internal static class RestApiService
  {
    private static readonly Lazy<HttpClient> _lazyClient = new(() =>
    {
      var baseAddress = $"http://localhost:{PortInfo.APIPort}/";

      return new HttpClient
      {
        BaseAddress = new Uri(baseAddress),
        Timeout = TimeSpan.FromSeconds(15)
      };
    });

    private static HttpClient Client => _lazyClient.Value;

    public static async Task<string?> GetReceiverNameAsync()
    {
      using var response = await SendRequestWithRetryAsync("api/v1/receiver");

      if (response?.IsSuccessStatusCode == true)
      {
        string payload = await response.Content.ReadAsStringAsync();
        var jsonPayload = JToken.Parse(payload);
        return jsonPayload["bluetoothName"]?.ToString() ?? string.Empty;
      }

      Debug.WriteLine($"[GetReceiverNameAsync] Failed to get receiver. Status: {response?.StatusCode}");
      return null;
    }

    public static async Task<bool> CheckReceiverConnectionAsync()
    {
      using var response = await SendRequestWithRetryAsync("api/v1/receiver");

      if (response?.IsSuccessStatusCode == true)
      {
        string payload = await response.Content.ReadAsStringAsync();
        var jsonPayload = JToken.Parse(payload);
        return jsonPayload["isConnected"]?.Value<bool>() ?? false;
      }

      Debug.WriteLine($"[CheckReceiverConnectionAsync] Failed to check connection. Status: {response?.StatusCode}");
      return false;
    }

    private static async Task<HttpResponseMessage?> SendRequestWithRetryAsync(string url)
    {
      HttpResponseMessage? response = null;

      try
      {
        string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

        Debug.WriteLine($"[SendRequestWithRetryAsync] First attempt for {url} with code for {DateTime.UtcNow:O}");
        response = await Client.GetAsync(url).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          Debug.WriteLine($"[SendRequestWithRetryAsync] First attempt failed with status {response.StatusCode}. Retrying...");

          response.Dispose();

          var pastTime = DateTime.UtcNow.AddSeconds(-1);
          var accessCodeGenerator = new AccessCodeGenerator(Values.AppID, pastTime);
          string previousAccessCode = accessCodeGenerator.AccessCode;

          Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", previousAccessCode);

          Debug.WriteLine($"[SendRequestWithRetryAsync] Second attempt for {url} with code for {pastTime:O}");
          response = await Client.GetAsync(url).ConfigureAwait(false);
        }
      }
      catch (Exception ex)
      {
        response?.Dispose();
        Debug.WriteLine($"[SendRequestWithRetryAsync] Exception caught: {ex.Message}");
        return null;
      }

      return response;
    }
  }
}
