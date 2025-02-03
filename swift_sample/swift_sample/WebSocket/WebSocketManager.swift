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
    //    Start the web socket session & connect to it
    guard let url = URL(string: "ws://localhost:\(locationPortV2)") else { return }
    
    webSocketTask = URLSession.shared.webSocketTask(with: url)
    webSocketTask?.resume()
    
    receiveMSG()
//    Receive the message and assign to the @Published variables
  }
  
  func disconnect() {
    webSocketTask?.cancel(with: .goingAway, reason: "Stop button pressed.".data(using: .utf8))
    DispatchQueue.main.async {
      self.lat = ""
      self.long = ""
      self.alt = ""
    }
//    Send cancel token to web socket task and blank the @Published variables
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
          self?.handleMsg(text: txt)
//          Call the handleMsg method on string message.
        case .data(_):
          print("Do something with binary data...")
//          We are not using binary data but Swift 6 requires us to do this anyways
        @unknown default:
          fatalError("Opps, something went wrong")
        }
        self?.receiveMSG()
//        Repeats the process. Much like a recursive.
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
