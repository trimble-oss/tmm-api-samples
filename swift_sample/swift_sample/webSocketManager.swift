//
//  webSocketManager.swift
//  swift_sample
//
//  Created by kea-build on 20/01/2025.
//
import Foundation

class WebSocketManager {
  private var wstask: URLSessionWebSocketTask?
  private var wsViewModel: webSocketViewModel
  
  init(wsViewModel: webSocketViewModel){
    self.wsViewModel = wsViewModel
  }
  
  func connect (apiPort: Int ) {
    if (apiPort != -1) {
      let url = URL(string: "ws://localhost:\(apiPort)/")!
      wstask = URLSession.shared.webSocketTask(with: url)
      wstask?.resume()
    }
    else {
      print("Invalid api port")
    }
  }
  
  func disconnect () {
    wstask?.cancel(with: .normalClosure, reason: nil)
    //    Disconnects WS with normal reasoning
  }
  
  private func receiveMSG() {
    wstask?.receive { [weak self] result in
      guard let self = self else { return }
      switch result {
      case .failure(let error):
        print("Web socket receiving error: \(error)")
      case .success(let message):
        switch message {
        case .string(let text):
          self.updateFields(with: text)
        case .data(let data):
          //          Switch case must be exhaustive if using Swift 6
          //          Therefore must include data even though the ws frame doesn't pass binary data
          print("Do something with data.")
        @unknown default:
          fatalError()
          //          @unknown gives a warning if default is run, otherwise nothing appears.
        }
        self.receiveMSG()
        //        Run the function again. This keeps going.
      }
    }
  }
  
  private func updateFields(with text: String) {
    let components = text.split(separator: ":")
    if components.count == 2 {
      let key = String(components[0])
      let value = String(components[1])
      DispatchQueue.main.async {
        switch key {
        case "lat":
          self.wsViewModel.lat = value
        case "long":
          self.wsViewModel.long = value
        case "alt":
          self.wsViewModel.alt = value
        default:
          break
        }
      }
    }
  }
  
  private func clearFields() {
    DispatchQueue.main.async {
      self.wsViewModel.lat = ""
      self.wsViewModel.long = ""
      self.wsViewModel.alt = ""
    }
  }
}
