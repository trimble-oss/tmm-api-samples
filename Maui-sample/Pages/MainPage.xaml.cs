namespace Maui_sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
    Title = $"TMM Test App (v{GetAppVersionString()})";
    // Retrieves the version number and displays as the title
  }

  private string GetAppVersionString()
  {
#if WINDOWS
    var version = Windows.ApplicationModel.Package.Current.Id.Version;
    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
#else
    return "";
#endif
  }
  public async Task<PermissionStatus> CheckAndRequestLocationPermission()
  {
    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

    if (status == PermissionStatus.Granted)
    {
      return status;
    }

    if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.WinUI)
    {
      await DisplayAlert("Location", "Please enable location permission in settings", "OK");
      return status;
    }

    if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
    {
      await DisplayAlert("Location", "Location permission is required to request locations", "OK");
    }

    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

    return status;
  }

  // The rest are just button functions that opens their respective interfaces
  private async void OnLocationButtonClicked(object sending, EventArgs e)
  {
    // Checks if location permission is on before opening the page. Pops an alert if it does.
    try
    {
      PermissionStatus status = await CheckAndRequestLocationPermission();

      if (status == PermissionStatus.Granted)
      {
        await Navigation.PushAsync(new LocationPage());
        SemanticScreenReader.Announce(LocationButton.Text);
      }
    }
    catch
    {
    }
  }

  private async void OnWebSocketLocationButtonClicked(object sender, EventArgs e)
  {
    try
    {
      if (LocationWebsocketClient.LocationPort != 0)
      {
        await Navigation.PushAsync(new LocationWebsocketPage(LocationWebsocketClient.LocationPort));
        SemanticScreenReader.Announce(WebSocketLocationButton.Text);
      }
      else
      {
        await DisplayAlert("Error", "Port value is not set. Please get the port value first.", "OK");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
  }

  private async void OnWebSocketLocationButtonClickedV2(object sender, EventArgs e)
  {
    try
    {
      if (LocationWebsocketClientV2.LocationV2Port != 0)
      {
        await Navigation.PushAsync(new LocationWebsocketPageV2(LocationWebsocketClientV2.LocationV2Port));
        SemanticScreenReader.Announce(WebSocketLocationButtonV2.Text);
      }
      else
      {
        await DisplayAlert("Error", "Port value is not set. Please get the port value first.", "OK");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
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
