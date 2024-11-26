namespace Maui_sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

  private async void OnLocationButtonClicked(object sending, EventArgs e)
  {
    try
    {
      await Navigation.PushAsync(new LocationPage());
      SemanticScreenReader.Announce(LocationButton.Text);
    }
    catch
    {
    }
  }

  private async void OnWebSocketLocationButtonClickedV2(object sender, EventArgs e)
  {
    try
    {
      await Navigation.PushAsync(new LocationWebsocketPageV2());
      SemanticScreenReader.Announce(WebSocketLocationButtonV2.Text);
    }
    catch
    {
    }
  }
  private async void OnWinRequestSchemeButtonClicked(object sender, EventArgs e)
  {
    try
    {
      await Navigation.PushAsync(new WinRequestSchemePage());
      SemanticScreenReader.Announce(WinRequestSchemeButton.Text);
    }
    catch
    {
    }
  }
  private async void OnRestfulApiButtonClicked(object sender, EventArgs e)
  {
    try
    {
      await Navigation.PushAsync(new RestfulApiPage());
      SemanticScreenReader.Announce(RestfulApiButton.Text);
    }
    catch
    {
    }
  }
}
