using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MauiSample.Models;

namespace MauiSample;

internal sealed class WebSocketService
{
  private static readonly JsonSerializerOptions _serializerOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  public event EventHandler<LocationV2DataMessage>? PositionReceived;

  public async Task ReadPositionsAsync(CancellationToken cancellationToken)
  {
    try
    {
      using ClientWebSocket client = new();
      await client.ConnectAsync(new Uri($"ws://localhost:{PortInfo.LocationV2Port}"), cancellationToken);

      while (!cancellationToken.IsCancellationRequested)
      {
        using MemoryStream messageBuffer = new();
        var buffer = new ArraySegment<byte>(new byte[1024]);
        WebSocketReceiveResult result;

        do
        {
          result = await client.ReceiveAsync(buffer, cancellationToken);

          if (result.MessageType == WebSocketMessageType.Close)
          {
            await client.CloseAsync(
              WebSocketCloseStatus.NormalClosure,
              "Closing",
              CancellationToken.None);
            return;
          }

          if (result.Count > 0)
          {
            messageBuffer.Write(buffer.Array!, buffer.Offset, result.Count);
          }
        }
        while (!result.EndOfMessage);

        if (result.MessageType != WebSocketMessageType.Text || messageBuffer.Length == 0)
        {
          continue;
        }

        string json = Encoding.UTF8.GetString(messageBuffer.ToArray());
        LocationV2DataMessage? position = JsonSerializer.Deserialize<LocationV2DataMessage>(
          json,
          _serializerOptions);

        if (position is not null)
        {
          PositionReceived?.Invoke(this, position);
        }
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      Debug.WriteLine("Position stream canceled.");
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"[ReadPositionsAsync] Error: {ex.Message}");
    }
  }
}
