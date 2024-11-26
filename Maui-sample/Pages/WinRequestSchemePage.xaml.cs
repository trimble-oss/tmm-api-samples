using System.Diagnostics;

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
  }

  private void Click_Register_Button(object sender, EventArgs e)
  {
    
    HttpClient client = new();
    string sampleAppID = SetSampleAppID().ToString();
    Debug.WriteLine(sampleAppID);
    string callbackUri = "\"myapp://response/?result=OK";
    Uri.EscapeDataString(callbackUri);
    Uri uri = new($"trimblemobilemanager://request/tmmRegister?applicationId={sampleAppID}&callback={callbackUri}");
    // trimblemobilemanager scheme is not supported
    client.BaseAddress = uri;
    try
    {
      HttpResponseMessage response = client.GetAsync(uri).Result;
      if (response.IsSuccessStatusCode)
      {
        Debug.WriteLine("StatusCode");
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
}
