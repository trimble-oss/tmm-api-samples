import Foundation

final class WebSocketService: @unchecked Sendable {
  private let host = "tmm-api-local.fieldsystems.trimble.com"
  private var webSocketTask: URLSessionWebSocketTask?
  private let decoder: JSONDecoder = {
    let decoder = JSONDecoder()
    decoder.keyDecodingStrategy = .convertFromSnakeCase
    return decoder
  }()

  func readPositions() -> AsyncStream<LocationV2DataMessage> {
    AsyncStream { continuation in
      let urlString =
        "wss://\(host):\(PortInfo.locationV2SecurePort)/locationV2"
      guard let url = URL(string: urlString) else {
        continuation.finish()
        return
      }

      let task = URLSession.shared.webSocketTask(with: url)
      self.webSocketTask = task
      task.resume()

      let receiveLoop = Task {
        while !Task.isCancelled {
          do {
            let message = try await task.receive()
            switch message {
            case .string(let text):
              if let data = text.data(using: .utf8),
                 let position = try? self.decoder.decode(LocationV2DataMessage.self, from: data)
              {
                continuation.yield(position)
              }
            case .data(let data):
              if let position = try? self.decoder.decode(LocationV2DataMessage.self, from: data) {
                continuation.yield(position)
              }
            @unknown default:
              break
            }
          } catch {
            if !Task.isCancelled {
              break
            }
          }
        }
        continuation.finish()
      }

      continuation.onTermination = { @Sendable _ in
        receiveLoop.cancel()
        task.cancel(with: .goingAway, reason: nil)
        self.webSocketTask = nil
      }
    }
  }

  func stop() {
    webSocketTask?.cancel(with: .goingAway, reason: nil)
    webSocketTask = nil
  }
}
