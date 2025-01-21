//
//  ContentView.swift
//  swift_sample
//
//  Created by Terence Chen on 14/01/2025.
//

import Foundation
import SwiftUI

struct LocationV2: Codable {
  var lat: Double
  var long: Double
  var alt: Double
}

struct ContentView: View {
  
  private var versionNumber: String {
    Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String
    ?? "Unknown"
  }
  private var headerText: String {
    "Swift Sample: Version \(versionNumber)"
  }
  
  // All need a value
  @State private var appID: String = ""
  @State private var receiverName: String = ""
  
  // Registration
  @EnvironmentObject var rViewModel: registrationViewModel
  
  // Web socket location
  @State private var wsButtonText: String = "Start Position Stream"
  @State private var latitude: String = ""
  @State private var longitude: String = ""
  @State private var altitude: String = ""
  @State private var wsTask: URLSessionWebSocketTask?
  
  var body: some View {
    VStack {
      // Header text fixed at the top
      Text(headerText)
        .font(.largeTitle)
        .padding([.top, .leading, .trailing], 20)
      
      Spacer()  // Spacer to push the content to the center
      Button("Clipboard workaround") {
        let pasteboard = UIPasteboard.general
        pasteboard.string = "Hello, World!"
        
        // Read from clipboard
        if let clipboardContent = pasteboard.string {
          print("Clipboard content: \(clipboardContent)")
        } else {
          print("No string content in clipboard")
        }
      } .buttonStyle(.borderedProminent)
      VStack {
        // Registration
        HStack {
          TextField("Enter your app's ID", text: $appID)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .padding()
          
          Button("Register") {
            register(with: appID)
          }
          .buttonStyle(.borderedProminent)
        }
        
        // REST API
        HStack {
          TextField("Receiver name", text: $receiverName)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .disabled(true)
            .padding()
          Button("Get Receiver", action: {
            receiverInfo(appID: appID, apiPort: rViewModel.apiPort) { bluetoothName in
              self.receiverName = bluetoothName
//              bluetoothName comes from the completion handler in the function
            }
          })
        }
        .buttonStyle(.borderedProminent)
        
        // WebSocket
        TextField("Latitude", text: $latitude)
          .textFieldStyle(RoundedBorderTextFieldStyle())
          .disabled(true)
          .padding()
        TextField("Longitude", text: $latitude)
          .textFieldStyle(RoundedBorderTextFieldStyle())
          .disabled(true)
          .padding()
        TextField("Altitude", text: $altitude)
          .textFieldStyle(RoundedBorderTextFieldStyle())
          .disabled(true)
          .padding()
        
        Button(action: {
          startWsConnection(locationPort: rViewModel.locationPort)
        }) {
          Text(wsButtonText)
        }
        .buttonStyle(.borderedProminent)
      }
      .padding()
      
      Spacer()  // Spacer to push the content to the center
    }
    .padding()
  }
  
  private func startWsConnection(locationPort: Int) {
  }

  private func stopWsConnection() {
    wsTask?.cancel(with: .goingAway, reason: nil)
    longitude = ""
    latitude = ""
    altitude = ""
  }

  private func updateFields(with text: String) {
  }
}

private func register(with appID: String) {
  //  returl
  let params: [String: String] =
  [
    "application_id": appID,
    "returl": "tmmapisample://com.trimble.tmmapisample",
  ]
  
  do {
    let jsonData = try JSONSerialization.data(withJSONObject: params, options: [])
    let jsonString = String(data: jsonData, encoding: .utf8)
    let base64Encoded = jsonString?.data(using: .utf8)?.base64EncodedString() ?? ""
    if let customUrl = URL(string: "tmmregister://?" + base64Encoded) {
      UIApplication.shared.open(customUrl, options: [:]) { success in
        // Sends the URL to TMM
        if success {
          print("The URL was delivered successfully.")
        } else {
          print("Failed to open the URL.")
        }
      }
    }
  } catch {
    print("Json serialization issue: \(error.localizedDescription)")
  }
}

private func receiverInfo(appID: String, apiPort: Int, completion: @escaping (String) -> Void) {
  guard let url = URL(string: "http://localhost:\(apiPort)/api/v1/receiver") else {
    print("Invalid URL")
    return
  }
  if apiPort == -1 {
    DispatchQueue.main.async {
      //          Updates to UI's must be completed on the main thread
      completion("Invalid api port or App is not registered")
    }
  }
  
  let utcTime = Date()
  guard let accessCode = AccessCodeGenerator.generateAccessCode(appID: appID, utcTime: utcTime) else {
    print("Failed to generate access code")
    return
  }
  
  var request = URLRequest(url: url)
  request.httpMethod = "GET"
  request.addValue("Basic \(accessCode)", forHTTPHeaderField: "Authorization")
  
  let task = URLSession.shared.dataTask(with: request) { data, response, error in
    if let error = error {
      print("Error: \(error.localizedDescription)")
      return
    }
    
    guard let data = data else {
      print("No data received")
      return
    }
    
    do {
      if let json = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any] {
        let bluetoothName = json["bluetoothName"] as? String ?? "App is not registered"
        DispatchQueue.main.async {
//          Updates to UI's must be completed on the main thread
          completion(bluetoothName)
        }
      } else {
        print("Invalid JSON format or app is not registered")
      }
    } catch {
      print("JSON parsing error: \(error.localizedDescription)")
    }
  }
  task.resume()
//  Starts connection
}

#Preview {
  ContentView()
}
