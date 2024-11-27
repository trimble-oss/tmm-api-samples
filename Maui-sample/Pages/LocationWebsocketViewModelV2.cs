using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui_sample
{
  internal partial class LocationWebsocketViewModelV2 : ObservableObject
  {
    private readonly LocationWebsocketClientV2 _client;
    private readonly CompositeDisposable _disposables = new();

    public LocationWebsocketViewModelV2(int port)
    {
      _client = new LocationWebsocketClientV2(port);
    }

    public async Task ConnectAsync()
    {
      try
      {
        IsWebsocketConnected = true;
        _client.ErrorOccurred.Subscribe(_ => OnWebsocketError()).DisposeWith(_disposables);
        _client.LocationDataReceivedV2.Subscribe(OnLocationDataReceived).DisposeWith(_disposables);
        await _client.ConnectAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}");
      }
    }

    public void Disconnect()
    {
      IsWebsocketConnected = false;
      _disposables.Clear();
      _client.Disconnect();
    }

    [ObservableProperty]
    private string _altitude;

    [ObservableProperty]
    private string _appVersion;

    [ObservableProperty]
    private string _bearing;

    [ObservableProperty]
    private string _battery;

    [ObservableProperty]
    private string _diffAge;

    [ObservableProperty]
    private string _diffId;

    [ObservableProperty]
    private string _diffStatus;

    [ObservableProperty]
    private string _geoidModel;

    [ObservableProperty]
    private string _gpsTimeStamp;

    [ObservableProperty]
    private string _hdop;

    [ObservableProperty]
    private string _hrms;

    [ObservableProperty]
    private bool _isWebsocketConnected;

    [ObservableProperty]
    private string _latitude;

    [ObservableProperty]
    private string _longitude;

    [ObservableProperty]
    private string _mockProvider;

    [ObservableProperty]
    private string _mslHeight;

    [ObservableProperty]
    private string _pdop;

    [ObservableProperty]
    private string _receiverModel;

    [ObservableProperty]
    private string _satelliteCount;

    [ObservableProperty]
    private string _satellites;

    [ObservableProperty]
    private string _satellitesInView;

    [ObservableProperty]
    private string _speed;

    [ObservableProperty]
    private string _subscriptionType;

    [ObservableProperty]
    private string _totalSatInUse;

    [ObservableProperty]
    private string _undulation;

    [ObservableProperty]
    private string _utcTimeStamp;

    [ObservableProperty]
    private string _vdop;

    [ObservableProperty]
    private string _vrms;

    [ObservableProperty]
    private string _sourceReferenceFrameId;

    [ObservableProperty]
    private string _sourceReferenceFrameName;

    [ObservableProperty]
    private string _sourceReferenceEpoch;

    [ObservableProperty]
    private string _sourceReferenceFrameEpoch;

    [ObservableProperty]
    private string _targetReferenceFrameEpoch;

    [ObservableProperty]
    private string _targetReferenceFrameName;

    [ObservableProperty]
    private string _imuAlignmentStatus;

    [ObservableProperty]
    private string _isTIP;

    [ObservableProperty]
    private string _pitch;

    [ObservableProperty]
    private string _roll;

    [ObservableProperty]
    private string _yaw;

    [ObservableProperty]
    private string _pitchPrecision;

    [ObservableProperty]
    private string _rollPrecision;

    [ObservableProperty]
    private string _yawPrecision;

    [ObservableProperty]
    private string _igsAntenna;

    [ObservableProperty]
    private string _antennaHeight;

    private void OnWebsocketError()
    {
      MainThread.BeginInvokeOnMainThread(() => IsWebsocketConnected = false);
    }

    private void OnLocationDataReceived(LocationDataV2 data)
    {
      MainThread.BeginInvokeOnMainThread(() => SetLocationData(data));
    }

    private void SetLocationData(LocationDataV2 data)
    {
      Altitude = data.Altitude.ToString();
      AppVersion = data.AppVersion;
      Bearing = data.Bearing.ToString();
      Battery = data.Battery?.ToString();
      DiffAge = data.DiffAge.ToString();
      DiffId = data.DiffId;
      DiffStatus = data.DiffStatus.ToString();
      GeoidModel = data.GeoidModel;
      GpsTimeStamp = data.GpsTimeStamp;
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
      TotalSatInUse = data.TotalSatInUse.ToString();
      Undulation = data.Undulation.ToString();
      UtcTimeStamp = data.UtcTimeStamp;
      Vdop = data.Vdop.ToString();
      Vrms = data.Vrms.ToString();
      SourceReferenceFrameName = data.SourceReferenceFrameName.ToString();
      SourceReferenceFrameEpoch = data.SourceReferenceFrameEpoch.ToString("F3");
      TargetReferenceFrameEpoch = data.TargetReferenceFrameEpoch.ToString("F3");
      TargetReferenceFrameName = data.TargetReferenceFrameName.ToString();

      ImuAlignmentStatus = GetImuAlignmentStatus(data.ImuAlignmentStatus);
      IsTIP = data.IsTIP.ToString();
      Pitch = data.Pitch?.ToString("F3");
      Roll = data.Roll?.ToString("F3");
      Yaw = data.Yaw?.ToString("F3");
      PitchPrecision = data.PitchPrecision?.ToString("F3");
      RollPrecision = data.RollPrecision?.ToString("F3");
      YawPrecision = data.YawPrecision?.ToString("F3");
      IgsAntenna = data.IGSAntenna;
      AntennaHeight = data.AntennaHeight.ToString("F3");

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
        0 => "Catalyst Free",
        1 => "Catalyst 60 ",
        2 => "Catalyst 30",
        3 => "Catalyst 10",
        4 => "Catalyst 1",
        5 => "Catalyst On Demand",
        100 => "Not a Catalyst GNSS Receiver",
        _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType))
      };
    }

    private static string GetImuAlignmentStatus(int imuAlignmentStatus)
    {
      return imuAlignmentStatus switch
      {
        0 => "Not available",
        1 => "Unaligned",
        2 => "Coarse",
        3 => "Fine",
        _ => throw new ArgumentOutOfRangeException(nameof(imuAlignmentStatus))
      };
    }
  }
}
