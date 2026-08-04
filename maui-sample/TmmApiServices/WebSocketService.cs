using System.Diagnostics;
using System.Net.Security;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MauiSample.Models;

namespace MauiSample;

internal sealed class WebSocketService
{
#if ANDROID
  private static readonly HashSet<string> s_localServerHosts = new(StringComparer.OrdinalIgnoreCase)
  {
    "localhost",
    "127.0.0.1",
    "tmm-api-local.fieldsystems.trimble.com"
  };
#endif

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
      Uri uri = new($"wss://tmm-api-local.fieldsystems.trimble.com:{PortInfo.LocationV2SecurePort}/locationV2");
      ConfigureLocalTlsValidation(client, uri);
      await client.ConnectAsync(uri, cancellationToken);

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

  private static void ConfigureLocalTlsValidation(ClientWebSocket socket, Uri uri)
  {
#if ANDROID
    // The local TMM API certificate chain is not in Android's system trust store.
    // Allow chain-only failures for known local hosts; hostname checks remain enforced.
    if (!string.Equals(uri.Scheme, "wss", StringComparison.OrdinalIgnoreCase)
        || !s_localServerHosts.Contains(uri.Host))
    {
      return;
    }

    socket.Options.RemoteCertificateValidationCallback = (_, _, _, sslPolicyErrors) =>
    {
      if (sslPolicyErrors == SslPolicyErrors.None)
      {
        return true;
      }

      // Allow only chain trust failures for known local test hosts.
      return (sslPolicyErrors & ~SslPolicyErrors.RemoteCertificateChainErrors) == 0;
    };
#endif
  }
}
