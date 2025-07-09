using System.Diagnostics;
using System.Net.Http.Headers;
using Maui_sample.AccessCode;
using Maui_sample.Models;
using Microsoft.Maui.Devices;
using Newtonsoft.Json.Linq;

namespace Maui_sample.RestApi
{
  internal class ReceiverMethods
  {
    private static readonly Lazy<HttpClient> _lazyClient = new(() =>
    {
      // Creates HttpClient for the receiver.
      var baseAddress = $"http://localhost:{PortInfo.APIPort}/";

      return new HttpClient
      {
        BaseAddress = new Uri(baseAddress),
        Timeout = TimeSpan.FromSeconds(15) 
      };
    });

    private static HttpClient Client => _lazyClient.Value;

    public static async Task GetReceiverAsync(MainPage mainPage)
    {
      // Ran after Receiver button is clicked. Will attempt to retrieve the connected receiver's name.
      string registrationStatus = mainPage._viewModel?.RegistrationStatus;
      if (registrationStatus != "OK")
      {
        mainPage._viewModel.ReceiverName = "Please register your app and try again.";
        return;
      }

      var response = await SendRequestWithRetryAsync("api/v1/receiver");

      if (response?.IsSuccessStatusCode == true)
      {
        string payload = await response.Content.ReadAsStringAsync();
        var JsonPayload = JToken.Parse(payload);
        // Gets the receiver's bluetooth name from the returned json.
        string? receiverName = JsonPayload["bluetoothName"]?.ToString();
        if (mainPage._viewModel != null)
        {
          mainPage._viewModel.ReceiverName = receiverName;
        }
      }
      else
      {
        Debug.WriteLine($"[GetReceiverAsync] Failed to get receiver. Status: {response?.StatusCode}");
        if (mainPage._viewModel != null)
        {
          // If app isn't registered, will ask user to register app.
          mainPage._viewModel.ReceiverName = "Failed to get receiver.";
        }
      }
    }

    public static async Task<bool> CheckReceiverConnection()
    {
      var response = await SendRequestWithRetryAsync("api/v1/receiver");

      if (response?.IsSuccessStatusCode == true)
      {
        var payload = await response.Content.ReadAsStringAsync();
        var JsonPayload = JToken.Parse(payload);
        // Checks whether receiver is connected. This function is ran when WebSocket tries to connect.
        return JsonPayload["isConnected"]?.Value<bool>() ?? false;
      }

      Debug.WriteLine($"[CheckReceiverConnection] Failed to check connection. Status: {response?.StatusCode}");
      return false;
    }

    private static async Task<HttpResponseMessage?> SendRequestWithRetryAsync(string url)
    {
      HttpResponseMessage? response = null;
      try
      {
        // Generates Access Code for the authorization header in the API call. Only valid for 1 second.
        string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

        Debug.WriteLine($"[SendRequestWithRetryAsync] First attempt for {url} with code for {DateTime.UtcNow:O}");
        response = await Client.GetAsync(url).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          Debug.WriteLine($"[SendRequestWithRetryAsync] First attempt failed with status {response.StatusCode}. Retrying...");

          var pastTime = DateTime.UtcNow.AddSeconds(-1);
          var acg = new AccessCodeGenerator(Values.AppID, pastTime);
          string previousAccessCode = acg.AccessCode;

          Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", previousAccessCode);

          Debug.WriteLine($"[SendRequestWithRetryAsync] Second attempt for {url} with code for {pastTime:O}");
          response = await Client.GetAsync(url).ConfigureAwait(false);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"[SendRequestWithRetryAsync] Exception caught: {ex.Message}");
        return null;
      }

      return response;
    }
  }
}
