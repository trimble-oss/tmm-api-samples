//
//  WebSocketManager.swift
//  swift_sample
//
//  Created by kea-build on 22/01/2025.
//

import Foundation
import Combine

struct WebSocketMessage: Codable {
  let latitude: Double
  let longitude: Double
  let altitude: Double
}

class WebSocketManager: ObservableObject {
  @Published var lat: String = ""
  @Published var long: String = ""
  @Published var alt: String = ""
  
  private var webSocketTask: URLSessionWebSocketTask?
  private var cancellables = Set<AnyCancellable>()
  private var pingTimer: Timer?
  
  func connect(with locationPortV2: Int) {
    guard let url = URL(string: "ws://localhost:\(locationPortV2)") else { return }
    
    webSocketTask = URLSession.shared.webSocketTask(with: url)
    webSocketTask?.resume()
    receiveMSG()
    startPingTimer()
  }
  
  func disconnect() {
    pingTimer?.invalidate()
    pingTimer = nil
    webSocketTask?.cancel(with: .goingAway, reason: "Stop button pressed.".data(using: .utf8))
    cancellables.forEach { $0.cancel() }
    cancellables.removeAll()
  }
  
  func receiveMSG() {
    webSocketTask?.receive { [weak self] result in
      switch result {
      case .failure(_):
        print("Failed to receive msg...")
      case .success(let msg):
        switch msg {
        case .string(let txt):
          print("Successfully received msg")
          self?.handleMsg(text: txt)
        case .data(_):
          print("Do something with binary data...")
        @unknown default:
          fatalError("Opps, something went wrong")
        }
        self?.receiveMSG()
//        Repeats the process. Much like a recursive
      }
    }
  }
  
  private func handleMsg(text: String) {
    guard let data = text.data(using: .utf8) else { return }
    
    do {
      let jsonString = try JSONDecoder().decode(WebSocketMessage.self, from: data)
      DispatchQueue.main.async {
        self.lat = String(jsonString.latitude)
        self.long = String(jsonString.longitude)
        self.alt = String(jsonString.altitude)
      }
    } catch {print("Error decoding: \(error)")}
  }
  
  private func startPingTimer() {
    pingTimer = Timer.scheduledTimer(withTimeInterval: 5.0, repeats: true) { [weak self] _ in
      self?.webSocketTask?.sendPing { err in
        if let err = err {
          print("Error in sending ping: \(err)")
        }
      }
    }
  }
}
