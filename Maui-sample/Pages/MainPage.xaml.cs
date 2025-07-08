using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Maui_sample.Models;
using Maui_sample.RestApi;
using Maui_sample.Utills;
using Maui_sample.WebSocket;
using Newtonsoft.Json.Linq;

namespace Maui_sample;

public partial class MainPage : ContentPage
{
  private bool _startStop = false;
  internal CancellationTokenSource? _cancellationTokenSource;
  internal MainPageViewModel? _viewModel => BindingContext as MainPageViewModel;
  private TaskCompletionSource<string> _registrationStatusCompletionSource = new();
  private readonly RegistrationAgent _registrationAgent = new();
  public MainPage()
  {
    InitializeComponent();
    WeakReferenceMessenger.Default.Register<UriMessage>(this, (r, m) => UseUri(m.Value));
    // Messenger allows App.xaml.cs to pass the callback uri to MainPage
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

#if WINDOWS
    try
    {
        _registrationStatusCompletionSource = new TaskCompletionSource<string>();

        // Launch the URI.
        await _registrationAgent.RegisterAsync(appID);

        // Await the result from the TaskCompletionSource that UseUri will complete.
        string registrationStatus = await _registrationStatusCompletionSource.Task;

        Debug.WriteLine($"Registration completed with status: {registrationStatus}");
        await DisplayAlert("Registration", $"Registration status: {registrationStatus}", "Okay");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"An error occurred during registration: {ex.Message}");
        await DisplayAlert("Error", "An unexpected error occurred during registration.", "OK");
    }
#else
    try
    {
      RegistrationDetails? registrationDetails = await _registrationAgent.RegisterAsync(appID);

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
    catch (Exception ex)
    {
      Debug.WriteLine($"An error occurred during registration: {ex.Message}");
      await DisplayAlert("Error", "An unexpected error occurred during registration.", "OK");
    }
#endif
  }


  private async void GetReceiverButton_Clicked(object sender, EventArgs e)
  {
    // second button in UI. Retrieves the connected receiver's name.
    await ReceiverMethods.GetReceiverAsync(this);
  }

  private readonly WebSocketMethods _webSocketMethods = new WebSocketMethods();
  private readonly ReceiverMethods _receiverMethods = new ReceiverMethods();

  private async void StartPositionStreamButton_Clicked(object sender, EventArgs e)
  {
    // Third button in UI. Will attempt to start position stream.
    // Checks registration status. Alert user to register app if not. Otherwise will try to get position data via WebSocket.
    
    if (_viewModel.RegistrationStatus == "OK")
    {
      // Checks if app is registered.
      if (await _receiverMethods.CheckReceiverConnection())
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

          _viewModel.Latitude = null;
          _viewModel.Longitude = null;
          _viewModel.Altitude = null;
        }
      }
      else
      {
        // Pop up window to ask user if they'd like to configure their receiver.
        // Otherwise will take them to connection window.
        bool userResponse = await DisplayAlert("Receiver not connected to TMM", "Make sure you have connected the Receiver to the OS's bluetooth.\n\nWould you like to configure your Receiver?", "Yes", "No");
        string requestId;
        string callback = Uri.EscapeDataString("tmmapisample://response/");
        if (userResponse)
        {
          requestId = "tmmOpenToConfiguration";
        }
        else
        {
          requestId = "tmmOpenToReceiverSelection";
        }
        await UtilMethods.checkRequest(requestId, callback);
      }
    }
    else
    {
      _viewModel.AreLabelsVisible = true;
      _viewModel.Messages = "Please register your app first or connect receiver";
    }
  }

  public void UseUri(Uri uri)
  {
    // Retrieves the callback URI and processes it.
    var queryParameters = uri.Query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(param => param.Split('='))
        .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));

    var jsonObject = new JObject();
    foreach (var param in queryParameters)
    {
      jsonObject[param.Key] = param.Value;
    }

    string responseJson = jsonObject.ToString();
    Debug.WriteLine(responseJson);
    var parsedJson = JObject.Parse(responseJson);

    // Gets the status and apiPort from the response Json. This can then be used for other functions in the app.
    string? registrationStatus = parsedJson["status"]?.ToString();
    string? apiPortString = parsedJson["apiPort"]?.ToString();

    if (_viewModel != null && !string.IsNullOrEmpty(registrationStatus))
    {
      _viewModel.RegistrationStatus = registrationStatus;
      _registrationStatusCompletionSource.SetResult(registrationStatus);
      // Sets the completion source. This is then passed to the Register button method when task is completed.
      if (int.TryParse(apiPortString, out int apiPort))
      {
        PortInfo.APIPort = apiPort;
      }
      // Task completed. Passes registrationStatus back to the Register Button.
    }
  }
}
