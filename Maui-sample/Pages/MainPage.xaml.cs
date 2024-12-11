using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Maui_sample.Models;
using Newtonsoft.Json.Linq;

namespace Maui_sample;

public partial class MainPage : ContentPage
{
  private bool _startStop = false;
  private MainPageViewModel? _viewModel => BindingContext as MainPageViewModel;
  public MainPage()
  {
    InitializeComponent();
  }

  private async void setAppID(string AppID)
  {
    await SecureStorage.Default.SetAsync("sampleAppID", AppID);
    // ID should not be commited to source control
  }

  private void StartStopButton_Clicked(object sender, EventArgs e)
  {
    _startStop = !_startStop;
    StartStopButton.Text = _startStop ? "Stop" : "Start";

    string? appID = _viewModel?.ApplicationID;
    // Run register URI. Check to see if the application ID is the same as the one stored in environment variables.
    if (appID != null && appID != "")
    {
      string responseUri = startOperations(appID);
      setAppID(appID);
      UseResponseUri(responseUri);
    }
    else
    {
      Debug.WriteLine("App ID is null.");
    }
  }

  private string startOperations(string AppID)
  {
#if WINDOWS
    try
    {
      // This should use the values gained from successful registration to do operations like location and web socket v2
      string requestId = "tmmRegister";
      string callback = Uri.EscapeDataString("sampleapp://response/tmmRegister");
      string applicationId = AppID;
      string requestUri = $"trimbleMobileManager://request/{requestId}?applicationId={applicationId}&callback={callback}";

      Debug.WriteLine("Starting registration");
      return requestUri;
      
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
      return string.Empty;
    }
#endif
    return string.Empty;
  }

  private void UseResponseUri(string responseUri)
  {
    int apiPort = PortInfo.APIPort;
    Debug.WriteLine($"Response URI: {responseUri}");
    Debug.WriteLine($"Api port: {apiPort}");
  }
}
