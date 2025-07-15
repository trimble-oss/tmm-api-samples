using System.Diagnostics;
using System.Text;
using System.Web;
using CommunityToolkit.Mvvm.Messaging;
using Maui_sample.Models;
using Maui_sample.RestApi;
using Maui_sample.Utills;
using Maui_sample.WebSocket;
using Microsoft.Maui.Devices;
using Newtonsoft.Json.Linq;

namespace Maui_sample;

public partial class MainPage : ContentPage
{
  private bool _startStop = false;
  internal CancellationTokenSource? _cancellationTokenSource;
  internal MainPageViewModel? _viewModel => BindingContext as MainPageViewModel;
  private TaskCompletionSource<string> _registrationStatusCompletionSource = new();

  private readonly WebSocketMethods _webSocketMethods = new WebSocketMethods();

  public MainPage()
  {
    RegistrationAgent.Instance.Initialize();
    InitializeComponent();
  }

  private async void RegisterButton_Clicked(object sender, EventArgs e)
  {
    string appID = Values.AppID;

    if (string.IsNullOrWhiteSpace(appID))
    {
      await DisplayAlert("Error", "Please enter an Application ID", "OK");
      return;
    }

    Debug.WriteLine("Starting registration with RegistrationAgent...");

    try
    {
      RegistrationDetails? registrationDetails = await RegistrationAgent.Instance.RegisterAsync(appID);

      if (registrationDetails != null && !string.IsNullOrEmpty(registrationDetails.RegistrationResult))
      {
        if (_viewModel != null)
        {
          _viewModel.RegistrationStatus = registrationDetails.RegistrationResult;
          PortInfo.APIPort = registrationDetails.ApiPort;
        }
        Debug.WriteLine($"Registration status: {registrationDetails.RegistrationResult}");
        await DisplayAlert("Registration", $"Registration status: {registrationDetails.RegistrationResult}", "Okay");
      }
      else
      {
        Debug.WriteLine("Registration failed or was cancelled.");
        await DisplayAlert("Registration", "Registration failed or was cancelled.", "Okay");
      }
    }
    catch (Exception)
    {
      Debug.WriteLine("Registration failed or was cancelled.");
      await DisplayAlert("Error", "An unexpected error occurred during registration.", "OK");
    }
  }

  private async void GetReceiverButton_Clicked(object sender, EventArgs e)
  {
    // second button in UI. Retrieves the connected receiver's name.
    await ReceiverMethods.GetReceiverAsync(this);
  }

  private async void StartPositionStreamButton_Clicked(object sender, EventArgs e)
  {
    // Third button in UI. Will attempt to start position stream.
    // Checks registration status. Alert user to register app if not. Otherwise will try to get position data via WebSocket.
    if (_viewModel?.IsRegistered == true)
    {
      // Checks if app is registered.
      if (await ReceiverMethods.CheckReceiverConnection())
      {
        // checks if receiver is connected.
        _startStop = !_startStop;
        StartPositionStreamButton.Text = _startStop ? "Stop position stream" : "Start position stream";
        if (_startStop)
        {
          _cancellationTokenSource = new CancellationTokenSource();
          await _webSocketMethods.ReadPositionsAsync(this, _cancellationTokenSource.Token);
        }
        else
        {
          // If button is pressed when streaming has begun, the stream will stop.
          // UI textboxes will be blanked.
          _cancellationTokenSource?.Cancel();
          _cancellationTokenSource?.Dispose();
          _cancellationTokenSource = null;

          if (_viewModel != null)
          {
            _viewModel.Latitude = null;
            _viewModel.Longitude = null;
            _viewModel.Altitude = null;
          }
        }
      }
      else
      {
        // Pop up window to ask user if they'd like to configure their receiver.
        // Otherwise will take them to connection window.
        bool userResponse = await DisplayAlert("Receiver not connected to TMM", "Make sure you have connected the Receiver to the OS's bluetooth.\n\nWould you like to configure your Receiver?", "Yes", "No");
        string requestId = userResponse ? "tmmOpenToConfiguration" : "tmmOpenToReceiverSelection";
        string callback = Uri.EscapeDataString("tmmapisample://response/");
        await UtilMethods.checkRequest(requestId, callback);
      }
    }
    else
    {
      if (_viewModel != null)
      {
        _viewModel.AreLabelsVisible = true;
        _viewModel.Messages = "Please register your app first or connect receiver";
      }
    }
  }

  public void UseUri(Uri uri)
  {
    try
    {
      string? registrationStatus = null;
      string? apiPortString = null;

      if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
      {
        string base64Json = uri.Query.TrimStart('?');
        byte[] jsonBytes = Convert.FromBase64String(base64Json);
        string responseJson = Encoding.UTF8.GetString(jsonBytes);
        var parsedJson = JObject.Parse(responseJson);
        registrationStatus = parsedJson["registrationResult"]?.ToString();
        apiPortString = parsedJson["apiPort"]?.ToString();
      }
      else
      {
        var queryParameters = HttpUtility.ParseQueryString(uri.Query);
        registrationStatus = queryParameters["status"];
        apiPortString = queryParameters["apiPort"];
      }

      if (_viewModel != null && !string.IsNullOrEmpty(registrationStatus))
      {
        _viewModel.RegistrationStatus = registrationStatus;
        _registrationStatusCompletionSource.TrySetResult(registrationStatus);

        if (int.TryParse(apiPortString, out int apiPort))
        {
          PortInfo.APIPort = apiPort;
        }
      }
      else
      {
        _registrationStatusCompletionSource.TrySetResult("Failed: No status in response");
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Error processing callback URI: {ex.Message}");
      _registrationStatusCompletionSource.TrySetException(ex);
    }
  }
}
