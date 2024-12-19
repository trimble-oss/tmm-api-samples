using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.Messaging;
using Maui_sample.AccessCode;
using Newtonsoft.Json.Linq;

namespace Maui_sample;

public partial class MainPage : ContentPage
{
  private bool _startStop = false;
  private CancellationTokenSource? _cancellationTokenSource;
  private MainPageViewModel? _viewModel => BindingContext as MainPageViewModel;
  private TaskCompletionSource<string> _registrationStatusCompletionSource = new TaskCompletionSource<string>();
  public MainPage()
  {
    InitializeComponent();
    WeakReferenceMessenger.Default.Register<UriMessage>(this, (r, m) => UseUri(m.Value));
    // Messenger allows App.xaml.cs to pass the callback uri to MainPage
  }

  private async void RegisterButton_Clicked(object sender, EventArgs e)
  {
      string? appID = _viewModel?.ApplicationID;

      if (string.IsNullOrWhiteSpace(appID) && appID == "")
      {
        Debug.WriteLine("App ID is null or empty.");
      }

    // Run register URI. Check to see if the application ID is the same as the one stored in environment variables.
    Debug.WriteLine("Starting registration...");
    string requestId = "tmmRegister";
    string callback = Uri.EscapeDataString("tmmapisample://response/tmmRegister");
    string applicationId = appID;
    string requestUri = $"trimbleMobileManager://request/{requestId}?applicationId={applicationId}&callback={callback}";
    if (await Launcher.Default.CanOpenAsync(requestUri))
    {
      Debug.WriteLine(await Launcher.Default.OpenAsync(requestUri));
    }
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
    string registrationStatus = _viewModel?.RegistrationStatus;
    if (registrationStatus == "OK")
    {
      int apiPort = 9637;

      HttpClient client = new HttpClient
      {
        BaseAddress = new Uri($"http://localhost:{apiPort}/"),
        Timeout = TimeSpan.FromSeconds(30)
      };
      string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

      var response = await client.GetAsync("api/v1/receiver").ConfigureAwait(false);
      var payload = await response.Content.ReadAsStringAsync();
      var JsonPayload = JToken.Parse(payload);
      //Debug.WriteLine($"Receiver info: {JsonPayload}");
      var receiverName = JsonPayload["bluetoothName"]?.ToString(Newtonsoft.Json.Formatting.Indented);
      Debug.WriteLine($"Name: {receiverName}");
      if (_viewModel != null)
      {
        _viewModel.ReceiverName = receiverName;
      }
    }
    else
    {
      Debug.WriteLine($"Registration status: {registrationStatus}");
      _viewModel.ReceiverName = "Please register your app and try again.";
    }
  }

  private async Task<bool> checkRequest(string requestUri)
  {
    if (await Launcher.Default.CanOpenAsync(requestUri))
    {
      bool resullt = await Launcher.Default.OpenAsync(requestUri);
      Debug.WriteLine($"Result: {resullt}");
      return resullt;
    }
    return false;
  }

  private async Task<int> GetPositionStreamPortAsync()
  {
    string? appID = _viewModel?.ApplicationID;
    int apiPort = 9637;

    // set up the HTTP client
    HttpClient client = new HttpClient
    {
      BaseAddress = new Uri($"http://localhost:{apiPort}/"),
      Timeout = TimeSpan.FromSeconds(30)
    };

    // generate the access code and put it in the authorization header
    string accessCode = AccessCodeGenerator.GenerateAccessCode(appID, DateTime.UtcNow);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

    // send the request
    string url = $"api/v1/positionStream?format=locationV2";
    HttpResponseMessage response = await client.GetAsync(url);
    if (!response.IsSuccessStatusCode)
      throw new Exception("Failed to get position stream port");

    // parse the response
    string jsonString = await response.Content.ReadAsStringAsync();
    JsonNode? jnode = JsonNode.Parse(jsonString);
    if(jnode is null)
    {
      throw new Exception("Failed to parse position stream port");
    }
    int port = jnode["port"]?.GetValue<int>() ?? 0;
    return port;
  }

  private async Task ReadPositionsAsync(CancellationToken cancel)
  {
    try
    {
      // query the position port
      int port = await GetPositionStreamPortAsync();

      // connect to the WebSocket
      using ClientWebSocket client = new ClientWebSocket();
      await client.ConnectAsync(new Uri($"ws://localhost:{port}"), cancel);

      if (!CheckReceiverConnection().Result)
      {
        _viewModel.AreLabelsVisible = true;
        _viewModel.Messages = "Please connect receiver.";
        // Open the TMM select device window if receiver is not connected
        try
        {
          string requestId = "tmmOpenToReceiverSelection";
          string callback = Uri.EscapeDataString("tmmapisample://response/");
          string requestUri = $"trimbleMobileManager://request/{requestId}?callback={callback}";
          if (!await checkRequest(requestUri))
          {
            _viewModel.Messages = "Failed to connect to receiver...";
            // Cancel task if receiver didn't connect after opening TMM.
            _cancellationTokenSource?.Cancel();
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
        }

        // Cancel task if receiver not connected
        _cancellationTokenSource?.Cancel();
        return;
      }
      else
      {
        _viewModel.AreLabelsVisible= false;
      }

      while (!cancel.IsCancellationRequested && CheckReceiverConnection().Result)
      {
        _viewModel.AreLabelsVisible = false;
        // read the next position
        var data = new ArraySegment<byte>(new byte[10240]);
        WebSocketReceiveResult result = await client.ReceiveAsync(data, cancel);
        // Stops here if no receiver info

        if (result.MessageType == WebSocketMessageType.Close)
        {
          await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
          break;
        }

        if (result.Count > 0 && result.MessageType == WebSocketMessageType.Text)
        {
          // parse the position data
          string jsonString = Encoding.UTF8.GetString(data.ToArray(), 0, result.Count);
          JsonNode? jnode = JsonNode.Parse(jsonString);
          if (jnode is not null)
          {
            double? latitude = jnode["latitude"]?.GetValue<double>();
            double? longitude = jnode["longitude"]?.GetValue<double>();
            double? altitude = jnode["altitude"]?.GetValue<double>();
            if (_viewModel != null)
            {
              // Shows lat, long and alt
              _viewModel.Latitude = latitude;
              _viewModel.Longitude = longitude;
              _viewModel.Altitude = altitude;
            }
          }
        }
      }
    }
    catch (TaskCanceledException)
    {
      // Catch Task Cancel Exception to stop app crashing when trying to stop the stream
      Debug.WriteLine("Task canceled");
    }
  }

  private async Task<bool> CheckReceiverConnection()
  {
    // Checks whether receiver is connected
    bool receiverConnected = false;
    int apiPort = 9637;

    HttpClient client = new HttpClient
    {
      BaseAddress = new Uri($"http://localhost:{apiPort}/"),
      Timeout = TimeSpan.FromSeconds(30)
    };
    string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

    var response = await client.GetAsync("api/v1/receiver").ConfigureAwait(false);
    var payload = await response.Content.ReadAsStringAsync();
    var JsonPayload = JToken.Parse(payload);
    var isConnected = JsonPayload["isConnected"]?.ToString(Newtonsoft.Json.Formatting.Indented);
    Debug.WriteLine($"Receiver connection status: {isConnected}");

    if (isConnected == "true")
    {
      receiverConnected = true;
    }
    return receiverConnected;
  }

  private async void StartPositionStreamButton_Clicked(object sender, EventArgs e)
  {
    // Checks registration status. Alert user to register app if not. Otherwise will try to get position via web socket
    if (_viewModel.RegistrationStatus == "OK")
    {
      _startStop = !_startStop;
      StartPositionStreamButton.Text = _startStop ? "Stop position stream" : "Start position stream";
      if (_startStop)
      {
        _cancellationTokenSource = new CancellationTokenSource();
        await ReadPositionsAsync(_cancellationTokenSource.Token);
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
      _viewModel.Messages = "Please register your app first";
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

    var responseJson = jsonObject.ToString();
    Debug.WriteLine(responseJson);
    var parsedJson = JObject.Parse(responseJson);

    var registrationStatus = parsedJson["status"]?.ToString();
    // Specifically grabs the status from the response Json.

    if (_viewModel != null && !string.IsNullOrEmpty(registrationStatus))
    {
      _viewModel.RegistrationStatus = registrationStatus;
      _registrationStatusCompletionSource.SetResult(registrationStatus);
      // Task completed. Passes registrationStatus back to the RegisterButton_Clicked
    }
  }
}
