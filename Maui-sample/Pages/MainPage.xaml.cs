using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using Maui_sample.AccessCode;
using Newtonsoft.Json.Linq;

namespace Maui_sample;

public partial class MainPage : ContentPage
{
  private bool _startStop = false;
  private CancellationTokenSource? _cancellationTokenSource;
  private MainPageViewModel? _viewModel => BindingContext as MainPageViewModel;
  public MainPage()
  {
    InitializeComponent();
  }

  private async void StartStopButton_Clicked(object sender, EventArgs e)
  {
    _startStop = !_startStop;
    StartStopButton.Text = _startStop ? "Stop" : "Start";

    if (_startStop)
    {
      _viewModel.AreLabelsVisible = true;
      string? appID = _viewModel?.ApplicationID;
      // Run register URI. Check to see if the application ID is the same as the one stored in environment variables.
      Debug.WriteLine("Starting registration...");
      string requestId = "tmmRegister";
      string callback = Uri.EscapeDataString("tmmapisample://response/tmmRegister");
      string applicationId = appID;
      string requestUri = $"trimbleMobileManager://request/{requestId}?applicationId={applicationId}&callback={callback}";
      Debug.WriteLine("");
      await checkRequest(requestUri);

      if (appID != null && appID != "")
      {
        _cancellationTokenSource = new CancellationTokenSource();
        await startOperations();
      }
      else
      {
        _cancellationTokenSource?.Cancel();
        Debug.WriteLine("App ID is null.");
      }
    }
    else
    {
      _cancellationTokenSource?.Cancel();
      Debug.WriteLine("Stopped operations.");
      _viewModel.AreLabelsVisible = false;
    }
  }

  private async Task startOperations()
  {
#if WINDOWS
    try
    {
      // This should use the values gained from successful registration to do operations like location and web socket v2
      if (_cancellationTokenSource != null)
      {
        Debug.WriteLine("Read position async running...");
        await ReadPositionsAsync(_cancellationTokenSource.Token);
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
#endif
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
    // query the position port
    int port = await GetPositionStreamPortAsync();

    // connect to the WebSocket
    using ClientWebSocket client = new ClientWebSocket();
    await client.ConnectAsync(new Uri($"ws://localhost:{port}"), cancel);

    if (!CheckReceiverConnection().Result)
    {
      _viewModel.Messages = "Please connect receiver.";
      // Open the TMM select device window
      try
      {
        string requestId = "tmmOpenToReceiverSelection";
        string callback = Uri.EscapeDataString("tmmapisample://response/");
        string requestUri = $"trimbleMobileManager://request/{requestId}?callback={callback}";
        if (!await checkRequest(requestUri))
        {
          _viewModel.Messages = "Failed to connect to receiver...";
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }
    }

    while (!cancel.IsCancellationRequested && CheckReceiverConnection().Result)
    {
      _viewModel.Messages = "";
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
          Debug.WriteLine($"Position: {latitude}, {longitude}, {altitude}");
          if (_viewModel != null)
          {
            _viewModel.Latitude = latitude;
            _viewModel.Longitude = longitude;
            _viewModel.Altitude = altitude;
          }
        }
      }
    }
  }

  private async Task<bool> CheckReceiverConnection()
  {
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
  public void UseUri(Uri uri)
  {
    // You can retrieve the Uri from the App.xaml.cs and process it
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

    var apiPort = parsedJson["apiPort"]?.ToString();
    Debug.WriteLine($"Api port: {apiPort}");
  }
}
