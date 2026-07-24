using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;
using ReactiveUI;

namespace MauiSample
{
  class MainPageViewModel : ReactiveObject
  {
    private readonly WebSocketService _webSocketService = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isStreaming;

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

    private string _positionStreamButtonText = "Start position stream";
    public string PositionStreamButtonText
    {
      get => _positionStreamButtonText;
      set => this.RaiseAndSetIfChanged(ref _positionStreamButtonText, value);
    }

    public bool IsRegistered => RegistrationStatus == "OK" || RegistrationStatus == "success";

    public IAsyncRelayCommand RegisterCommand { get; }
    public IAsyncRelayCommand GetReceiverCommand { get; }
    public IAsyncRelayCommand TogglePositionStreamCommand { get; }

    public MainPageViewModel()
    {
      _messages = string.Empty;
      _applicationID = Values.AppID;
      _receiverName = string.Empty;
      _registrationStatus = string.Empty;
      _webSocketService.PositionReceived += OnPositionReceived;

      // AsyncRelayCommand (vs ReactiveCommand) keeps CanExecuteChanged on the UI thread,
      // which avoids WinUI COMExceptions when the button's disabled visual state updates.
      RegisterCommand = new AsyncRelayCommand(RegisterAsync);
      GetReceiverCommand = new AsyncRelayCommand(GetReceiverAsync);
      TogglePositionStreamCommand = new AsyncRelayCommand(TogglePositionStreamAsync);
    }

    private async Task RegisterAsync()
    {
      string appID = Values.AppID;

      if (string.IsNullOrWhiteSpace(appID))
      {
        await DisplayAlertAsync("Error", "Please enter an Application ID", "OK");
        return;
      }

      Debug.WriteLine("Starting registration with RegistrationAgent...");

      try
      {
        RegistrationDetails? registrationDetails = await PlatformRequestService.Instance.RegisterAsync(appID);

        if (registrationDetails != null && !string.IsNullOrEmpty(registrationDetails.RegistrationResult))
        {
          RegistrationStatus = registrationDetails.RegistrationResult;

          if (string.Equals(registrationDetails.RegistrationResult, "OK", StringComparison.OrdinalIgnoreCase))
          {
            // Update the PortInfo class with the port information received from the registration process.
            // If TMM is unable to assign a server to its default port,
            // it will assign a new port starting incrementally at 9650.
            PortInfo.LocationPort = registrationDetails.LocationPort;
            PortInfo.LocationSecurePort = registrationDetails.LocationSecurePort;
            PortInfo.ApiPort = registrationDetails.ApiPort;
            PortInfo.ApiSecurePort = registrationDetails.ApiSecurePort;
            PortInfo.LocationV2Port = registrationDetails.LocationV2Port;
            PortInfo.LocationV2SecurePort = registrationDetails.LocationV2SecurePort;
          }
          Debug.WriteLine($"Registration status: {registrationDetails.RegistrationResult}");
          await DisplayAlertAsync("Registration", $"Registration status: {registrationDetails.RegistrationResult}", "Okay");
        }
        else
        {
          Debug.WriteLine("Registration failed or was cancelled.");
          await DisplayAlertAsync("Registration", "Registration failed or was cancelled.", "Okay");
        }
      }
      catch (Exception)
      {
        Debug.WriteLine("Registration failed or was cancelled.");
        await DisplayAlertAsync("Error", "An unexpected error occurred during registration.", "OK");
      }
    }

    private async Task TogglePositionStreamAsync()
    {
      // Will attempt to start position stream.
      // Checks registration status. Alert user to register app if not. Otherwise will try to get position data via WebSocket.
      if (IsRegistered)
      {
        // Checks if app is registered.
        if (await CheckReceiverConnection())
        {
          // checks if receiver is connected.
          _isStreaming = !_isStreaming;
          PositionStreamButtonText = _isStreaming ? "Stop position stream" : "Start position stream";
          if (_isStreaming)
          {
            // Do not await: keeping the command free lets the user tap again to stop.
            _cancellationTokenSource = new CancellationTokenSource();
            _ = ReadPositionsAsync(_cancellationTokenSource.Token);
          }
          else
          {
            // If button is pressed when streaming has begun, the stream will stop.
            // UI textboxes will be blanked.
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            Latitude = null;
            Longitude = null;
            Altitude = null;
          }
        }
        else
        {
          // Pop up window to ask user if they'd like to configure their receiver.
          // Otherwise will take them to connection window.
          await DisplayAlertAsync("Receiver Not Connected",
            "Connect to a receiver in TMM to start streaming positions.",
            "Okay");
        }
      }
      else
      {
        AreLabelsVisible = true;
        Messages = "Please register your app first or connect receiver";
      }
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

    private static async Task DisplayAlertAsync(string title, string message, string cancel)
    {
      Page? page = Application.Current?.MainPage;
      if (page is null)
      {
        Debug.WriteLine("Application.Current.MainPage is null. Cannot display alert.");
        return;
      }

      await page.DisplayAlert(title, message, cancel);
    }
  }
}
