using System;
using System.Diagnostics;
//using Windows.System;

namespace Maui_sample;

public partial class WinRequestSchemePage : ContentPage
{
	public WinRequestSchemePage()
	{
		InitializeComponent();
	}
  private async Task SetSampleAppID()
  {
    string appID = await SecureStorage.Default.GetAsync("sampleAppID");

    if (appID == null)
    {
      await SecureStorage.Default.SetAsync("sampleAppID", Environment.GetEnvironmentVariable("SampleAppID"));
    }
    // appID is correct value
  }

  public async void Click_Register_Button(object sender, EventArgs e)
  {
    string sampleAppID = SetSampleAppID().ToString();
    Debug.WriteLine(sampleAppID);
    string callbackUri = "\"sampleapp://response/?result=OK";
    Uri.EscapeDataString(callbackUri);
    string requestString = $"trimbleMobileManager://request/tmmRegister?applicationId={sampleAppID}&callback={callbackUri}";
    Uri requestUri = new Uri(requestString);
    Debug.WriteLine(requestUri);

    if (await Launcher.CanOpenAsync(requestUri))
    {
      bool result = await Launcher.OpenAsync(requestUri);
    }
  }
}
