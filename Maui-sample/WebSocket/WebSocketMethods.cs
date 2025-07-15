using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Text;
using Maui_sample.AccessCode;
using System.Net.Http.Headers;
using Maui_sample.Utills;
using Maui_sample.Models;
using Maui_sample.RestApi;
using Microsoft.Maui.Devices;

namespace Maui_sample.WebSocket
{
  public class WebSocketMethods
  {
    internal async Task ReadPositionsAsync(MainPageViewModel vm, CancellationTokenSource cancel)
    {
      // Called when app tries to connect to the WebSocket.
      // Needs the cancellation token used in main page to disconnect WebSocket when receiver not connected.
      try
      {
        // query for the WebSocket position port.
        int port = await GetPositionStreamPortAsync(vm);
        if (port == 0)
        {
          Debug.WriteLine("Failed to get a valid position stream port.");
          return;
        }

        // connect to the WebSocket using the aforementioned WebSocket position port.
        using ClientWebSocket client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), cancel.Token);

        while (!cancel.IsCancellationRequested)
        {
          // Will continue to run as long as the WebSocket and receiver are connected.
          vm.AreLabelsVisible = false;
          // Gets the next set of position data.
          var data = new ArraySegment<byte>(new byte[10240]);
          WebSocketReceiveResult result = await client.ReceiveAsync(data, cancel.Token);
          // Stops here if no receiver info

          if (result.MessageType == WebSocketMessageType.Close)
          {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            break;
          }

          if (result.Count > 0 && result.MessageType == WebSocketMessageType.Text)
          {
            // parse the position data.
            string jsonString = Encoding.UTF8.GetString(data.ToArray(), 0, result.Count);
            JsonNode? jnode = JsonNode.Parse(jsonString);
            if (jnode is not null)
            {
              double? latitude = jnode["latitude"]?.GetValue<double>();
              double? longitude = jnode["longitude"]?.GetValue<double>();
              double? altitude = jnode["altitude"]?.GetValue<double>();
              if (vm != null)
              {
                // Updates the UI to show the lat, long and alt data.
                vm.Latitude = latitude;
                vm.Longitude = longitude;
                vm.Altitude = altitude;
              }
            }
          }
        }
      }
      catch (TaskCanceledException)
      {
        // Catch Task Cancel Exception to stop app crashing when trying to stop the stream.
        Debug.WriteLine("Task canceled");
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"[ReadPositionsAsync] Error: {ex.Message}");
      }
    }

    private static async Task<int> GetPositionStreamPortAsync(MainPageViewModel vm)
    {
      try
      {
        // This will return the port number to the app so it can connect to the WebSocket.
        string? appID = vm.ApplicationID;

        string baseAddress = $"http://localhost:{PortInfo.APIPort}/";

        // set up the HTTP client for WebSocket.
        using HttpClient client = new HttpClient
        {
          BaseAddress = new Uri(baseAddress),
          Timeout = TimeSpan.FromSeconds(30)
        };

        // generate the access code for authorization header in the API.
        string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);

        // send the request to position stream API.
        string url = $"api/v1/positionStream?format=locationV2";
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        // parse the response if successfullly received.
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonNode? jnode = JsonNode.Parse(jsonString);
        if (jnode is null)
        {
          throw new Exception("Failed to parse position stream port");
        }
        int port = jnode["port"]?.GetValue<int>() ?? 0;
        return port;
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"[GetPositionStreamPortAsync] Error: {ex.Message}");
        return 0; 
      }
    }
  }
}
