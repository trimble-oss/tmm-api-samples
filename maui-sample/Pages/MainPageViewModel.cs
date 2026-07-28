using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using MauiSample.AccessCode;
using MauiSample.Models;
using ReactiveUI;

namespace MauiSample
{
  class MainPageViewModel : ReactiveObject
  {
    private readonly WebSocketService _webSocketService = new();
    private CancellationTokenSource? _cancellationTokenSource;

    private bool _isConnecting;
    public bool IsConnecting => _isConnecting;

    private bool _isConnected;
    public bool IsConnected => _isConnected;

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
      get => _statusMessage;
      set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    private string _latitudeText = string.Empty;
    public string LatitudeText
    {
      get => _latitudeText;
      set => this.RaiseAndSetIfChanged(ref _latitudeText, value);
    }

    private string _longitudeText = string.Empty;
    public string LongitudeText
    {
      get => _longitudeText;
      set => this.RaiseAndSetIfChanged(ref _longitudeText, value);
    }

    private string _altitudeText = string.Empty;
    public string AltitudeText
    {
      get => _altitudeText;
      set => this.RaiseAndSetIfChanged(ref _altitudeText, value);
    }

    private string _accuracyText = string.Empty;
    public string AccuracyText
    {
      get => _accuracyText;
      set => this.RaiseAndSetIfChanged(ref _accuracyText, value);
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

    public IAsyncRelayCommand ConnectCommand { get; }
    public IAsyncRelayCommand DisconnectCommand { get; }

    public MainPageViewModel()
    {
      _applicationID = Values.AppID;
      _webSocketService.PositionReceived += OnPositionReceived;

      // AsyncRelayCommand (vs ReactiveCommand) keeps CanExecuteChanged on the UI thread,
      // which avoids WinUI COMExceptions when the button's disabled visual state updates.
      ConnectCommand = new AsyncRelayCommand(ConnectAsync, () => !IsConnecting && !IsConnected);
      DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, () => !IsConnecting && IsConnected);
    }

    private async Task ConnectAsync()
    {
      if (string.IsNullOrWhiteSpace(Values.AppID))
      {
        StatusMessage = "Please enter an Application ID.";
        await DisplayAlertAsync("Error", "Please enter an Application ID", "OK");
        return;
      }

      SetConnectionState(isConnecting: true, isConnected: false);
      StatusMessage = "Connecting...";

      try
      {
        StatusMessage = "Getting public key...";
        string? publicKey = await TryGetPublicKeyAsync();
        if (publicKey is null)
        {
          // Failing to get the public key means TMM is not running, or the
          // REST API port is not on the default port.
          // Use Register to start TMM and query the current TMM server ports.
          StatusMessage = "Registering...";
          if (!await TryRegisterAsync())
          {
            StatusMessage = "Registration failed.";
            return;
          }

          StatusMessage = "Getting public key again...";
          publicKey = await TryGetPublicKeyAsync();
          if (publicKey is null)
          {
            StatusMessage = "Failed to get public key.";
            return;
          }
        }

        StatusMessage = "Getting receiver info...";
        ReceiverInfo? receiver = await RestApiService.GetReceiverAsync();
        if (receiver is null)
        {
          StatusMessage = "Failed to get receiver info.";
          return;
        }

        if (!receiver.IsReceiverConfigured)
        {
          StatusMessage = "Receiver not configured. Select a receiver in TMM.";
          bool openTmm = await DisplayConfirmAsync(
            "Receiver Not Configured",
            "Connect to a receiver in TMM to start streaming positions.",
            "Open TMM",
            "Cancel");
          if (openTmm)
          {
            await PlatformRequestService.Instance.ShowReceiverSelectionAsync();
          }
          return;
        }

        if (!receiver.IsConnected)
        {
          StatusMessage = "Connecting to GNSS receiver...";
          await RestApiService.PutReceiverAsync(true);

          receiver = await RestApiService.GetReceiverAsync();
          if (receiver is null || !receiver.IsConnected)
          {
            StatusMessage = "Failed to connect to GNSS receiver.";
            return;
          }
        }

        StatusMessage = "Starting position stream...";
        _cancellationTokenSource = new CancellationTokenSource();
        _ = ReadPositionsAsync(_cancellationTokenSource.Token);

        StatusMessage = "Connected.";
        SetConnectionState(isConnecting: false, isConnected: true);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"[ConnectAsync] Error: {ex.Message}");
        StatusMessage = "Connection failed.";
        await DisplayAlertAsync("Error", "An unexpected error occurred while connecting.", "OK");
        SetConnectionState(isConnecting: false, isConnected: false);
      }
      finally
      {
        if (_isConnecting)
        {
          SetConnectionState(isConnecting: false, isConnected: false);
        }
      }
    }

    private Task DisconnectAsync()
    {
      SetConnectionState(isConnecting: true, isConnected: false);
      StatusMessage = "Disconnecting...";

      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource?.Dispose();
      _cancellationTokenSource = null;

      ClearLocationData();
      StatusMessage = "Disconnected.";
      SetConnectionState(isConnecting: false, isConnected: false);
      return Task.CompletedTask;
    }

    private async Task<string?> TryGetPublicKeyAsync()
    {
      try
      {
        string? publicKey = await RestApiService.GetPublicKeyAsync();
        if (publicKey is not null)
        {
          AccessCodeV2.SetPublicKey(publicKey);
        }
        return publicKey;
      }
      catch
      {
        return null;
      }
    }

    private async Task<bool> TryRegisterAsync()
    {
      Debug.WriteLine("Starting registration with RegistrationAgent...");

      try
      {
        RegistrationDetails? registrationDetails = await PlatformRequestService.Instance.RegisterAsync(Values.AppID);

        if (registrationDetails is null || string.IsNullOrEmpty(registrationDetails.RegistrationResult))
        {
          Debug.WriteLine("Registration failed or was cancelled.");
          return false;
        }

        if (string.Equals(registrationDetails.RegistrationResult, "OK", StringComparison.OrdinalIgnoreCase))
        {
          PortInfo.LocationPort = registrationDetails.LocationPort;
          PortInfo.LocationSecurePort = registrationDetails.LocationSecurePort;
          PortInfo.ApiPort = registrationDetails.ApiPort;
          PortInfo.ApiSecurePort = registrationDetails.ApiSecurePort;
          PortInfo.LocationV2Port = registrationDetails.LocationV2Port;
          PortInfo.LocationV2SecurePort = registrationDetails.LocationV2SecurePort;
        }

        Debug.WriteLine($"Registration status: {registrationDetails.RegistrationResult}");
        return string.Equals(registrationDetails.RegistrationResult, "OK", StringComparison.OrdinalIgnoreCase)
          || string.Equals(registrationDetails.RegistrationResult, "success", StringComparison.OrdinalIgnoreCase);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"[TryRegisterAsync] Error: {ex.Message}");
        return false;
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
        LatitudeText = FormatCoordinate(position.Latitude);
        LongitudeText = FormatCoordinate(position.Longitude);
        AltitudeText = FormatDistance(position.Altitude);
        AccuracyText = FormatDistance(position.Hrms);
      });
    }

    private static string FormatCoordinate(double? value)
    {
      return value.HasValue ? value.Value.ToString("F8") + "°" : string.Empty;
    }

    private static string FormatDistance(double? value)
    {
      return value.HasValue ? value.Value.ToString("F3") + " m" : string.Empty;
    }

    private void ClearLocationData()
    {
      LatitudeText = string.Empty;
      LongitudeText = string.Empty;
      AltitudeText = string.Empty;
      AccuracyText = string.Empty;
    }

    private void SetConnectionState(bool isConnecting, bool isConnected)
    {
      MainThread.BeginInvokeOnMainThread(() =>
      {
        _isConnecting = isConnecting;
        _isConnected = isConnected;
        this.RaisePropertyChanged(nameof(IsConnecting));
        this.RaisePropertyChanged(nameof(IsConnected));
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
      });
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

    private static async Task<bool> DisplayConfirmAsync(string title, string message, string accept, string cancel)
    {
      Page? page = Application.Current?.MainPage;
      if (page is null)
      {
        Debug.WriteLine("Application.Current.MainPage is null. Cannot display alert.");
        return false;
      }

      return await page.DisplayAlert(title, message, accept, cancel);
    }
  }
}
