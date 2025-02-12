//
//  WebSocketManager.swift
//  swift_sample
//
//  Created by kea-build on 22/01/2025.
//

import Foundation
import Combine

struct WebSocketMessage: Decodable {
  let latitude: Double
  let longitude: Double
  let altitude: Double
//  Variables to decode the json in
}

class WebSocketManager: ObservableObject {
  @Published var lat: String = ""
  @Published var long: String = ""
  @Published var alt: String = ""
//  Varibles to assign the WS message to and pass back to the TextField
  
  private var webSocketTask: URLSessionWebSocketTask?
  
  func connect(with locationPortV2: Int) {
    //    Start the WebSocket session & connect to it using the Location Port
    guard let url = URL(string: "ws://localhost:\(locationPortV2)") else { return }
    
    webSocketTask = URLSession.shared.webSocketTask(with: url)
    webSocketTask?.resume()
    
//    Function that receives the message and assign to the @Published variables
    receiveMSG()
  }
  
  func disconnect() {
//    Send cancel token to WebSocket task and blank the @Published variables
    webSocketTask?.cancel(with: .goingAway, reason: "Stop button pressed.".data(using: .utf8))
    DispatchQueue.main.async {
      self.lat = ""
      self.long = ""
      self.alt = ""
    }
  }
  
  func receiveMSG() {
    webSocketTask?.receive { [weak self] result in
//      weak self releases memory once job is finished. Reduces the chances of causing (memory leaks)
      switch result {
      case .failure(_):
        print("Failed to receive msg...")
      case .success(let msg):
        switch msg {
        case .string(let txt):
//          Call the handleMsg method on string message.
          self?.handleMsg(text: txt)
        case .data(_):
//          We are not using binary data but Swift 6 requires us to do this anyways
          print("Do something with binary data...")
        @unknown default:
          fatalError("Opps, something went wrong")
        }
//        Repeats the process. Much like a recursive.
        self?.receiveMSG()
      }
    }
  }
  
  private func handleMsg(text: String) {
    //    Decodes the json from the data
    guard let data = text.data(using: .utf8) else { return }

    do {
      let jsonString = try JSONDecoder().decode(WebSocketMessage.self, from: data)
      DispatchQueue.main.async {
        self.lat = String(jsonString.latitude)
        self.long = String(jsonString.longitude)
        self.alt = String(jsonString.altitude)
      }
    } catch {
      print("Error decoding: \(error)")
    }
  }
}
