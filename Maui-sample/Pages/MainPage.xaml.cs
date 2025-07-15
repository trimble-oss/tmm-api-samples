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
  internal MainPageViewModel? ViewModel => BindingContext as MainPageViewModel;
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
        if (ViewModel != null)
        {
          ViewModel.RegistrationStatus = registrationDetails.RegistrationResult;
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
    if (ViewModel is not null)
    {
      await ReceiverMethods.GetReceiverAsync(ViewModel);
    }
  }

  private async void StartPositionStreamButton_Clicked(object sender, EventArgs e)
  {
    // Third button in UI. Will attempt to start position stream.
    // Checks registration status. Alert user to register app if not. Otherwise will try to get position data via WebSocket.
    if (ViewModel?.IsRegistered == true)
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
          await _webSocketMethods.ReadPositionsAsync(ViewModel, _cancellationTokenSource);
        }
        else
        {
          // If button is pressed when streaming has begun, the stream will stop.
          // UI textboxes will be blanked.
          _cancellationTokenSource?.Cancel();
          _cancellationTokenSource?.Dispose();
          _cancellationTokenSource = null;

          if (ViewModel != null)
          {
            ViewModel.Latitude = null;
            ViewModel.Longitude = null;
            ViewModel.Altitude = null;
          }
        }
      }
      else
      {
        // Pop up window to ask user if they'd like to configure their receiver.
        // Otherwise will take them to connection window.
        await DisplayAlert("Receiver Not Connected",
          "Connect to a receiver in TMM to start streaming positions.",
          "Okay");
      }
    }
    else
    {
      if (ViewModel != null)
      {
        ViewModel.AreLabelsVisible = true;
        ViewModel.Messages = "Please register your app first or connect receiver";
      }
    }
  }
}
