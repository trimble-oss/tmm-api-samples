using System.Reactive;
using MauiSample.Models;
using ReactiveUI;

namespace MauiSample
{
  class MainPageViewModel : ReactiveObject
  {
    private readonly WebSocketService _webSocketService = new();

    private bool _areLabelsVisible;
    public bool AreLabelsVisible
    {
      get => _areLabelsVisible;
      set => this.RaiseAndSetIfChanged(ref _areLabelsVisible, value);
    }

    private string _messages;
    public string Messages
    {
      get => _messages;
      set => this.RaiseAndSetIfChanged(ref _messages, value);
    }

    private string _receiverName;
    public string ReceiverName
    {
      get => _receiverName;
      set => this.RaiseAndSetIfChanged(ref _receiverName, value);
    }

    private double? _latitude;
    public double? Latitude
    {
      get => _latitude;
      set => this.RaiseAndSetIfChanged(ref _latitude, value);
    }

    private double? _longitude;
    public double? Longitude
    {
      get => _longitude;
      set => this.RaiseAndSetIfChanged(ref _longitude, value);
    }

    private double? _altitude;
    public double? Altitude
    {
      get => _altitude;
      set => this.RaiseAndSetIfChanged(ref _altitude, value);
    }

    private string _applicationID = string.Empty;

    public string ApplicationID
    {
      get => _applicationID;
      set
      {
        if (_applicationID != value)
        {
          _applicationID = value;
          Values.AppID = value;
          this.RaisePropertyChanged();
        }
      }
    }

    private string _registrationStatus;
    public string RegistrationStatus
    {
      get => _registrationStatus;
      set => this.RaiseAndSetIfChanged(ref _registrationStatus, value);
    }

    public bool IsRegistered => RegistrationStatus == "OK" || RegistrationStatus == "success";

    public MainPageViewModel()
    {
      _messages = string.Empty;
      _applicationID = Values.AppID;
      _receiverName = string.Empty;
      _registrationStatus = string.Empty;
      _webSocketService.PositionReceived += OnPositionReceived;
    }

    public Task ReadPositionsAsync(CancellationToken cancellationToken)
    {
      return _webSocketService.ReadPositionsAsync(cancellationToken);
    }

    private void OnPositionReceived(object? sender, LocationV2DataMessage position)
    {
      MainThread.BeginInvokeOnMainThread(() =>
      {
        AreLabelsVisible = false;
        Latitude = position.Latitude;
        Longitude = position.Longitude;
        Altitude = position.Altitude;
      });
    }

    public async Task GetReceiverAsync()
    {
      // Ran after Receiver button is clicked. Will attempt to retrieve the connected receiver's name.
      if (IsRegistered == false)
      {
        ReceiverName = "Please register your app and try again.";
        return;
      }

      var receiver = await RestApiService.GetReceiverAsync();

      if (receiver is not null)
      {
        ReceiverName = receiver.BluetoothName ?? string.Empty;
      }
      else
      {
        ReceiverName = "Failed to get receiver.";
      }
    }

    public async Task<bool> CheckReceiverConnection()
    {
      // Checks whether receiver is connected when WebSocket tries to connect.
      if (IsRegistered == false)
      {
        return false;
      }

      ReceiverInfo? receiver = await RestApiService.GetReceiverAsync();
      return receiver?.IsConnected ?? false;
    }
  }
}
