using System.Diagnostics;
using System.Net.Http.Headers;
using Maui_sample.Models;
using Newtonsoft.Json.Linq;

namespace Maui_sample.RestApi
{
  internal class ReceiverMethods
  {
    public static async Task GetReceiverAsync(MainPage mainPage)
    {
      // Ran after Receiver button is clicked. Will attempt to retrieve name.
      string registrationStatus = mainPage._viewModel?.RegistrationStatus;
      if (registrationStatus == "OK")
      {

        HttpClient client = new HttpClient
        {
          BaseAddress = new Uri($"http://localhost:{PortInfo.APIPort}/"),
          Timeout = TimeSpan.FromSeconds(30)
        };
        string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

        HttpResponseMessage response = await client.GetAsync("api/v1/receiver").ConfigureAwait(false);
        string payload = await response.Content.ReadAsStringAsync();
        var JsonPayload = JToken.Parse(payload);
        string? receiverName = JsonPayload["bluetoothName"]?.ToString(Newtonsoft.Json.Formatting.Indented);
        Debug.WriteLine($"Name: {receiverName}");
        if (mainPage._viewModel != null)
        {
          mainPage._viewModel.ReceiverName = receiverName;
        }
      }
      else
      {
        Debug.WriteLine($"Registration status: {registrationStatus}");
        mainPage._viewModel.ReceiverName = "Please register your app and try again.";
      }
    }
    public async Task<bool> CheckReceiverConnection()
    {
      // Checks whether receiver is connected. This function is ran when web socket tries to connect.
      bool receiverConnected = false;

      HttpClient client = new HttpClient
      {
        BaseAddress = new Uri($"http://localhost:{PortInfo.APIPort}/"),
        Timeout = TimeSpan.FromSeconds(30)
      };
      string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

      var response = await client.GetAsync("api/v1/receiver").ConfigureAwait(false);
      var payload = await response.Content.ReadAsStringAsync();
      var JsonPayload = JToken.Parse(payload);
      var isConnected = JsonPayload["isConnected"]?.ToString(Newtonsoft.Json.Formatting.Indented);
      Debug.WriteLine($"Receiver connection status: {isConnected}");

      if (isConnected == "true")
      {
        receiverConnected = true;
      }
      return receiverConnected;
    }
  }
}
