using System.Timers;
using Timer = System.Timers.Timer;

namespace Maui_sample;

public partial class LocationPage : ContentPage
{
  private readonly Timer _timer;
  private CancellationTokenSource _cancelTokenSource;

  public LocationPage()
  {
    InitializeComponent();

    _timer = new Timer { Interval = 1000 };
    _timer.Elapsed += OnLocationTimerElapsed;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    _timer.Start();
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    _cancelTokenSource?.Cancel();
  }

  private void OnLocationTimerElapsed(object sender, ElapsedEventArgs e)
  {
    MainThread.BeginInvokeOnMainThread(OnLocationTimerElapsed);
  }

  private async void OnLocationTimerElapsed()
  {
    try
    {
      _timer.Stop();
      _cancelTokenSource = new CancellationTokenSource();

      GeolocationRequest request = new(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
      Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

      if (location != null)
      {
        mockProviderValue.Text = location.IsFromMockProvider.ToString();
        latitudeValue.Text = location.Latitude.ToString("F6");
        longitudeValue.Text = location.Longitude.ToString("F6");
        altitudeValue.Text = GetValue(location.Altitude);
        altitudeReferenceValue.Text = location.AltitudeReferenceSystem.ToString();
        accuracyValue.Text = GetValue(location.Accuracy);
        bearingValue.Text = GetValue(location.Course);
        speedValue.Text = GetValue(location.Speed);
      }

      _timer.Start();
    }
    catch
    {
    }
    finally
    {
      _cancelTokenSource?.Dispose();
      _cancelTokenSource = null;
    }
  }

  private static string GetValue(double? value)
  {
    return value.HasValue ? value.Value.ToString("F2") : "N/A";
  }
}
