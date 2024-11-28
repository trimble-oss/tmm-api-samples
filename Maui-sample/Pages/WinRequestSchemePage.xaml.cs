using System;
using System.Diagnostics;

namespace Maui_sample;

public partial class WinRequestSchemePage : ContentPage
{
  public WinRequestSchemePage()
  {
    InitializeComponent();
  }
  private async Task<string> SampleAppID()
  {
    string appID = await SecureStorage.Default.GetAsync("sampleAppID");
    if (appID == null)
    {
      await SecureStorage.Default.SetAsync("sampleAppID", Environment.GetEnvironmentVariable("SampleAppID"));
    }
    return appID;
  }

  public async void Click_Register_Button(object sender, EventArgs e)
  {
    try
    {
      string sampleAppID = await SampleAppID();
      string callbackUri = "sampleapp://response";
      Uri.EscapeDataString(callbackUri);
      string requestString = $"trimbleMobileManager://request/tmmRegister?applicationId={sampleAppID}&callback={callbackUri}";
      Uri requestUri = new Uri(requestString);
      Debug.WriteLine(requestUri);

      if (await Launcher.CanOpenAsync(requestUri))
      {
        bool result = await Launcher.OpenAsync(requestUri);
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
}
