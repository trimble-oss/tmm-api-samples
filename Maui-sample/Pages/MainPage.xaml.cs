using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using Maui_sample.RestApi;
using Maui_sample.WebSocket;
using Maui_sample.Utills;
using Maui_sample.Models;

namespace Maui_sample;

public partial class MainPage : ContentPage
{
  private bool _startStop = false;
  internal CancellationTokenSource? _cancellationTokenSource;
  internal MainPageViewModel? _viewModel => BindingContext as MainPageViewModel;
  private TaskCompletionSource<string> _registrationStatusCompletionSource = new TaskCompletionSource<string>();
  public MainPage()
  {
    InitializeComponent();
    WeakReferenceMessenger.Default.Register<UriMessage>(this, (r, m) => UseUri(m.Value));
    // Messenger allows App.xaml.cs to pass the callback uri to MainPage
  }

  private async void RegisterButton_Clicked(object sender, EventArgs e)
  {
    //Register button. Should be first thing ran in the app.
    // Run register URI. Check to see if the application ID is the same as the one stored in environment variables.
    string? appID = _viewModel?.ApplicationID;

    if (string.IsNullOrWhiteSpace(appID) && appID == "")
    {
      Debug.WriteLine("App ID is null or empty.");
    }
    
    Debug.WriteLine("Starting registration...");
    string requestId = "tmmRegister";
    string callback = Uri.EscapeDataString("tmmapisample://response/tmmRegister");
    string applicationId = appID;
    string requestUri = $"trimbleMobileManager://request/{requestId}?applicationId={applicationId}&callback={callback}";
    await UtilMethods.checkRequest(requestUri);
    string registrationStatus = await _registrationStatusCompletionSource.Task;
    // Waits until the registration Uri is returned before continuing

    if (registrationStatus == "OK")
    {
      Debug.WriteLine($"Registration status: {registrationStatus}");
    }
    else if (registrationStatus == "NoNetwork")
    {
      Debug.WriteLine("No Network");
    }
    else
    {
      Debug.WriteLine("UnAuthorized");
    }
  }

  private async void GetReceiverButton_Clicked(object sender, EventArgs e)
  {
    // second button in UI. Retrieves the receiver's name.
    await ReceiverMethods.GetReceiverAsync(this);
  }

  private WebSocketMethods _webSocketMethods = new WebSocketMethods();
  private ReceiverMethods _receiverMethods = new ReceiverMethods();

  private async void StartPositionStreamButton_Clicked(object sender, EventArgs e)
  {
    //Third button in UI. Will attempt to start position stream.
    // Checks registration status. Alert user to register app if not. Otherwise will try to get position via web socket.
    if (_viewModel.RegistrationStatus == "OK")
    {
      if (await _receiverMethods.CheckReceiverConnection())
      {
        _startStop = !_startStop;
        StartPositionStreamButton.Text = _startStop ? "Stop position stream" : "Start position stream";
        if (_startStop)
        {
          _cancellationTokenSource = new CancellationTokenSource();
          await _webSocketMethods.ReadPositionsAsync(this, _cancellationTokenSource.Token);
        }
        else
        {
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
        _viewModel.AreLabelsVisible = true;
        _viewModel.Messages = "Please connect receiver";
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

    string? registrationStatus = parsedJson["status"]?.ToString();
    // Specifically grabs the status from the response Json.
    string? apiPortString = parsedJson["apiPort"]?.ToString();

    if (_viewModel != null && !string.IsNullOrEmpty(registrationStatus))
    {
      _viewModel.RegistrationStatus = registrationStatus;
      _registrationStatusCompletionSource.SetResult(registrationStatus);
      if (int.TryParse(apiPortString, out int apiPort))
      {
        PortInfo.APIPort = apiPort;
      }
      // Task completed. Passes registrationStatus back to the RegisterButton_Clicked
    }
  }
}
