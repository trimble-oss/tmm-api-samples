using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Maui_sample
{
  internal class LocationWebsocketClient
  {
    private readonly int _port;
    public static int LocationPort { get; set; }
    private readonly Subject<Unit> _websocketErrorOccurred = new();
    private readonly Subject<LocationData> _locationDataReceived = new();
    private ClientWebSocket _client;
    private CancellationTokenSource _tokenSource;

    public IObservable<Unit> ErrorOccurred => _websocketErrorOccurred;
    public IObservable<LocationData> LocationDataReceived => _locationDataReceived;

    public LocationWebsocketClient(int port)
    {
      _port = port;
    }

    public async void ConnectAsync()
    {
      try
      {
        _tokenSource = new();
        _client = new ClientWebSocket();
        await _client.ConnectAsync(new Uri($"ws://localhost:{_port}"), _tokenSource.Token);

        Console.WriteLine("Connected to websocket");

        await Task.Factory.StartNew(async () =>
        {
          try
          {
            while (true)
            {
              await ReadMessage();
            }
          }
          catch (TaskCanceledException)
          {
            _client.Dispose();
            _client = null;
          }
        }, _tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
      }
      catch
      {
        _websocketErrorOccurred.OnNext(Unit.Default);
      }
    }

    public void Disconnect()
    {
      _tokenSource?.Cancel();
      _tokenSource = null;
    }

    private async Task ReadMessage()
    {
      WebSocketReceiveResult result;
      string json = string.Empty;
      var message = new ArraySegment<byte>(new byte[4096]);

      do
      {
        result = await _client.ReceiveAsync(message, _tokenSource.Token);

        if (result.MessageType != WebSocketMessageType.Text)
        {
          break;
        }

        byte[] messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
        json += Encoding.UTF8.GetString(messageBytes);
      }
      while (!result.EndOfMessage);

      try
      {
        var locationData = JsonConvert.DeserializeObject<LocationData>(json);
        _locationDataReceived.OnNext(locationData);
      }
      catch (Exception e)
      {
        Console.WriteLine("Error deserializing position : {0}", e);
      }
    }
  }
}
