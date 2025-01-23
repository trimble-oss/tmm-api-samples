using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Text;
using Maui_sample.AccessCode;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Maui_sample.Utills;
using Maui_sample.Models;

namespace Maui_sample.WebSocket
{
  public class WebSocketMethods
  {
    internal async Task ReadPositionsAsync(MainPage mainPage, CancellationToken cancel)
    {
      try
      {
        // query the position port
        int port = await GetPositionStreamPortAsync(mainPage);

        // connect to the WebSocket
        using ClientWebSocket client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), cancel);

        if (!CheckReceiverConnection().Result)
        {
          mainPage._viewModel.AreLabelsVisible = true;
          mainPage._viewModel.Messages = "Please connect receiver.";
          // Open the TMM select device window if receiver is not connected
          try
          {
            string requestId = "tmmOpenToReceiverSelection";
            string callback = Uri.EscapeDataString("tmmapisample://response/");
            string requestUri = $"trimbleMobileManager://request/{requestId}?callback={callback}";
            if (!await UtilMethods.checkRequest(requestUri))
            {
              mainPage._viewModel.Messages = "Failed to connect to receiver...";
              // Cancel task if receiver didn't connect after opening TMM.
              mainPage._cancellationTokenSource?.Cancel();
            }
          }
          catch (Exception ex)
          {
            Debug.WriteLine(ex.Message);
          }

          // Cancel task if receiver not connected
          mainPage._cancellationTokenSource?.Cancel();
          return;
        }
        else
        {
          mainPage._viewModel.AreLabelsVisible = false;
        }

        while (!cancel.IsCancellationRequested && CheckReceiverConnection().Result)
        {
          mainPage._viewModel.AreLabelsVisible = false;
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
              if (mainPage._viewModel != null)
              {
                // Shows lat, long and alt
                mainPage._viewModel.Latitude = latitude;
                mainPage._viewModel.Longitude = longitude;
                mainPage._viewModel.Altitude = altitude;
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

    internal async Task<int> GetPositionStreamPortAsync(MainPage mainPage)
    {
      string? appID = mainPage._viewModel?.ApplicationID;

      // set up the HTTP client
      HttpClient client = new HttpClient
      {
        BaseAddress = new Uri($"http://localhost:{PortInfo.APIPort}/"),
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
      if (jnode is null)
      {
        throw new Exception("Failed to parse position stream port");
      }
      int port = jnode["port"]?.GetValue<int>() ?? 0;
      return port;
    }

    private async Task<bool> CheckReceiverConnection()
    {
      // Checks whether receiver is connected
      bool receiverConnected = false;

      HttpClient client = new HttpClient
      {
        BaseAddress = new Uri($"http://localhost:{PortInfo.APIPort}/"),
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
  }
}
