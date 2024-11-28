using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui_sample
{
  internal partial class LocationWebsocketViewModel : ObservableObject
  {
    private readonly LocationWebsocketClient _client;
    private readonly CompositeDisposable _disposables = new();

    public LocationWebsocketViewModel(int port)
    {
      _client = new LocationWebsocketClient(port);
    }

    public void Connect()
    {
      IsWebsocketConnected = true;
      _client.ErrorOccurred.Subscribe(_ => OnWebsocketError()).DisposeWith(_disposables);
      _client.LocationDataReceived.Subscribe(OnLocationDataReceived).DisposeWith(_disposables);
      _client.ConnectAsync();
    }

    public void Disconnect()
    {
      IsWebsocketConnected = false;
      _disposables.Clear();
      _client.Disconnect();
    }

    [ObservableProperty]
    private bool _isWebsocketConnected;

    [ObservableProperty]
    private string _latitude;

    [ObservableProperty]
    private string _longitude;

    [ObservableProperty]
    private string _altitude;

    [ObservableProperty]
    private string _speed;

    [ObservableProperty]
    private string _bearing;

    [ObservableProperty]
    private string _accuracy;

    [ObservableProperty]
    private string _verticalAccuracy;

    [ObservableProperty]
    private string _pdop;

    [ObservableProperty]
    private string _hdop;

    [ObservableProperty]
    private string _vdop;

    [ObservableProperty]
    private string _diffAge;

    [ObservableProperty]
    private string _diffId;

    [ObservableProperty]
    private string _diffStatus;

    [ObservableProperty]
    private string _hrms;

    [ObservableProperty]
    private string _vrms;

    [ObservableProperty]
    private string _receiverModel;

    [ObservableProperty]
    private string _mockProvider;

    [ObservableProperty]
    private string _appVersion;

    [ObservableProperty]
    private string _geoidModel;

    [ObservableProperty]
    private string _mslHeight;

    [ObservableProperty]
    private string _undulation;

    [ObservableProperty]
    private string _utcTime;

    [ObservableProperty]
    private string _gpsTimeStamp;

    [ObservableProperty]
    private string _utcTimeStamp;

    [ObservableProperty]
    private string _subscriptionType;

    [ObservableProperty]
    private string _satelliteCount;

    [ObservableProperty]
    private string _totalSatInUse;

    [ObservableProperty]
    private string _satellitesInView;

    [ObservableProperty]
    private string _satellites;

    [ObservableProperty]
    private string _battery;

    private void OnWebsocketError()
    {
      MainThread.BeginInvokeOnMainThread(() => IsWebsocketConnected = false);
    }

    private void OnLocationDataReceived(LocationData data)
    {
      MainThread.BeginInvokeOnMainThread(() => SetLocationData(data));
    }

    private void SetLocationData(LocationData data)
    {
      Accuracy = data.Accuracy.ToString();
      Altitude = data.Altitude.ToString();
      AppVersion = data.AppVersion;
      Bearing = data.Bearing.ToString();
      Battery = data.Battery?.ToString();
      DiffAge = data.DiffAge.ToString();
      DiffId = data.DiffId;
      DiffStatus = data.DiffStatus.ToString();
      GpsTimeStamp = data.GpsTimeStamp;
      GeoidModel = data.GeoidModel;
      Hdop = data.Hdop.ToString();
      Hrms = data.Hrms.ToString();
      Latitude = data.Latitude.ToString();
      Longitude = data.Longitude.ToString();
      MockProvider = data.MockProvider;
      MslHeight = data.MslHeight.ToString();
      Pdop = data.Pdop.ToString();
      ReceiverModel = data.ReceiverModel;
      SatellitesInView = data.TotalSatellitesInView.ToString();
      SatelliteCount = data.SatelliteCount.ToString();
      Speed = data.Speed.ToString();
      SubscriptionType = GetSubscriptionType(data.SubscriptionType);
      Undulation = data.Undulation.ToString();
      UtcTime = data.UtcTime.ToString();
      UtcTimeStamp = data.UtcTimeStamp;
      Vdop = data.Vdop.ToString();
      VerticalAccuracy = data.VerticalAccuracyMeters.ToString();
      Vrms = data.Vrms.ToString();

      if (data.Satellites != null)
      {
        StringBuilder stringBuilder = new();
        foreach (LocationSatellite satellite in data.Satellites)
        {
          stringBuilder.Append(satellite.ToString());
        }

        Satellites = stringBuilder.ToString();
      }
    }

    private static string GetSubscriptionType(int subscriptionType)
    {
      return subscriptionType switch
      {
        0 => "Free",
        1 => "Meter",
        2 => "Submeter",
        3 => "Decimeter",
        4 => "Precision",
        5 => "Precision On Demand",
        100 => "GNSS receiver",
        _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType))
      };
    }
  }
}
