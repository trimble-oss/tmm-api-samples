using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
#if WINDOWS
using Maui_sample.Platforms.Windows;
// Conditional compilation otherwise there is error with detecting the Platforms namespace

namespace Maui_sample;

public partial class WinRequestSchemePage : ContentPage
{
  public WinRequestSchemePage()
  {
    InitializeComponent();
  }

  // Retrieves the registered AppID (obtained from Trimble) from the platform's respective secure storage and if it isn't saved then it will create it.
  private async Task<string> SampleAppID()
  {
    string appID = await SecureStorage.Default.GetAsync("sampleAppID");
    if (appID == null)
    {
      await SecureStorage.Default.SetAsync("sampleAppID", Values.AppID);
      // The ID should not be commited to source control
    }
    return appID;
  }

  // All Windows requests will have similar format: trimbleMobileManager://request/nameofRequest?applicationId=yourAppID&callback=callbackURI
  public async void TmmRegisterClicked(object sender, EventArgs e)
  {
    // Responsible for registration
    try
    {
      string sampleAppID = await SampleAppID();
      string requestID = "tmmRegister";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?applicationId={sampleAppID}&callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        // At the moment it will only display the result of the response
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmLoginClicked(object sender, EventArgs e)
  {
    //
    try
    {
      string sampleAppID = await SampleAppID();
      string requestID = "tmmLogin";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?applicationId={sampleAppID}&callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmOpenToLoginPageClicked(object sender, EventArgs e)
  {
    try
    {
      string sampleAppID = await SampleAppID();
      string requestID = "tmmOpenToLoginPage";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmOpenToAntennaHeightClicked(object sender, EventArgs e)
  {
    // Gets antenna info
    try
    {
      string requestID = "tmmOpenToAntennaHeight";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmOpenToConfigurationClicked(object sender, EventArgs e)
  {
    // Opens configuration page
    try
    {
      string requestID = "tmmOpenToConfiguration";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmOpenToReceiverSelectionClicked(object sender, EventArgs e)
  {
    // Gets reciever info
    try
    {
      string requestID = "tmmOpenToReceiverSelection";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmCorrectionSettingsClicked(object sender, EventArgs e)
  {
    // Opens correction settings
    try
    {
      string requestID = "tmmCorrectionSettings";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmOpenToSkyplotClicked(object sender, EventArgs e)
  {
    // Opens skyplot page
    try
    {
      string requestID = "tmmOpenToSkyplot";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmOnDemandClicked(object sender, EventArgs e)
  {
    try
    {
      string requestID = "tmmOnDemand";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
  public async void TmmFileClicked(object sender, EventArgs e)
  {
    try
    {
      string requestID = "tmmFileLocations";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public async void TmmRefreshUserTokenClicked(object sender, EventArgs e)
  {
    // Refresh tokens
    try
    {
      string requestID = "tmmRefreshUserToken";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
  public async void TmmSocketServerPortClicked(object sender, EventArgs e)
  {
    try
    {
      string requestID = "tmmSocketServerPort";
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/{requestID}?callback={callbackUri}";
      Uri requestUri = new Uri(requestString);

      await WinRequestSchemeService.CallAsync(requestString, callbackUri, async (responseJson) =>
      {
        await DisplayRegisterResultAsync(responseJson);
      });
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  // Default response if the response to the request is null or empty or an issue with parsing
  public async Task DefaultResponseDisplayAsync(string responseJson)
  {
    if (string.IsNullOrEmpty(responseJson))
    {
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        await DisplayAlert("Error", "Response JSON is null or empty", "Ok");
      });
      return;
    }

    JObject jsonObject;
    try
    {
      jsonObject = JObject.Parse(responseJson);
    }
    catch (Exception ex)
    {
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        await DisplayAlert("Error", "Failed to parse response JSON: " + ex.Message, "Ok");
      });
      return;
    }

    StringBuilder stringBuilder = new StringBuilder();
    var keys = new Dictionary<string, string>
    {
        { "id", "ID" },
        { "status", "Status" },
        { "message", "Message" }
    };

    foreach (var key in keys)
    {
      var value = jsonObject[key.Key]?.ToString();
      stringBuilder.AppendFormat("{0}: {1}\n", key.Value, value ?? "null");
    }

    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
      await DisplayAlert("Response", stringBuilder.ToString(), "OK");
    });
  }

  // Displays Register Result. Handle the response here as desired.
  public async Task DisplayRegisterResultAsync(string response)
  {
    try
    {
      var jsonObject = JObject.Parse(response);

      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("ID: {0}\n", jsonObject.GetValue("id").ToObject<string>())
                   .AppendFormat("Status: {0}\n", jsonObject.GetValue("status").ToObject<string>())
                   .AppendFormat("Message: {0}\n", jsonObject.GetValue("message").ToObject<string>())
                   .AppendFormat("Registration Result: {0}\n", jsonObject.GetValue("registrationResult").ToObject<string>())
                   .AppendFormat("API Port: {0}\n", jsonObject.GetValue("apiPort").ToObject<string>())
                   .AppendFormat("Location V2 Port: {0}\n", jsonObject.GetValue("locationV2Port").ToObject<string>());

      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        await DisplayAlert("Register Result", stringBuilder.ToString(), "Ok");
      });
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing JSON response: {ex.Message}");
    }
  }
}
#endif
