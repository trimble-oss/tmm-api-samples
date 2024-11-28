namespace Maui_sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
    Title = $"TMM Test App (v{GetAppVersionString()})";
  }

  private string GetAppVersionString()
  {
#if WINDOWS
    var version = Windows.ApplicationModel.Package.Current.Id.Version;
    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

#elif ANDROID
    var context = Android.App.Application.Context;
    var packageInfo = OperatingSystem.IsAndroidVersionAtLeast(33)
      ? context.PackageManager?.GetPackageInfo(context.PackageName, Android.Content.PM.PackageManager.PackageInfoFlags.Of(0L))
#pragma warning disable CS0618
      : context.PackageManager?.GetPackageInfo(context.PackageName, 0);
#pragma warning restore CS0618

    return packageInfo == null ? "not available" : packageInfo.VersionName;

    //#elif IOS
    //    return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();

#else
    return "";

#endif
  }

  private async void OnLocationButtonClicked(object sending, EventArgs e)
  {
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

  public async Task<PermissionStatus> CheckAndRequestLocationPermission()
  {
    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

    if (status == PermissionStatus.Granted)
    {
      return status;
    }

    if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
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
}
