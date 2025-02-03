//
//  ContentView.swift
//  swift_sample
//
//  Created by Terence Chen on 14/01/2025.
//

import Foundation
import SwiftUI

struct ContentView: View {
  
  private var versionNumber: String {
    Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String
    ?? "Unknown"
  }
//  Gets version number from properties and assign to headerText
  private var headerText: String {
    "Swift Sample: Version \(versionNumber)"
  }
  
  @State private var appID: String = ""
  @State private var receiverName: String = ""
  
  // Registration - Allows the UI to receive the callback
  @EnvironmentObject var rViewModel: registrationViewModel
  
//  Receiver
  @State private var returnBluetoothName: Bool = true
  @State private var receiverClass = ReceiverClass()
  
  // Web socket location
  @State private var isConnectedInt: Int = 0
  @State private var isReceiverConnected: Bool = false
  @StateObject private var wsManager = WebSocketManager()
  
  var body: some View {
    VStack {
      // Header text fixed at the top
      Text(headerText)
        .font(.largeTitle)
        .padding([.top, .leading, .trailing], 20)
      
      Spacer()  // Spacer to push the content to the center
     
      VStack {
        // Registration textfield and button
        HStack {
          TextField("Enter your app's ID", text: $appID)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .padding()
          
          Button("Register") {
            register(with: appID)
          }
          .buttonStyle(.borderedProminent)
        }
        
        // REST API - Receiver
        HStack {
          TextField("Receiver name", text: $receiverName)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .disabled(true)
            .padding()
          Button("Get Receiver", action: {
//            Shows the receiver name in the textfield
            returnBluetoothName = true
            if returnBluetoothName {
              receiverClass.receiverInfo(appID: appID, apiPort: rViewModel.apiPort, returnReceiverName: returnBluetoothName) { (bluetoothName: String?, isConnectedJson: Int?) in
                if bluetoothName != nil {
                  self.receiverName = bluetoothName ?? ""
                }
              }
//              bluetoothName comes from the completion handler in the function
            }
          })
        }
        .buttonStyle(.borderedProminent)
        
        // WebSocket - Position stream
        TextField("Latitude", text: $wsManager.lat)
          .textFieldStyle(RoundedBorderTextFieldStyle())
          .disabled(true)
          .padding()
        TextField("Longitude", text: $wsManager.long)
          .textFieldStyle(RoundedBorderTextFieldStyle())
          .disabled(true)
          .padding()
        TextField("Altitude", text: $wsManager.alt)
          .textFieldStyle(RoundedBorderTextFieldStyle())
          .disabled(true)
          .padding()
        
        Button(action: {
//          Position stream. When pressed, will attempt to start the connection. If receiver is not connected will switch to TMM.
//          Text will switch over and start showing positions from receiver if it's connected.
//          If already started, the button's text will switch over again and stop streaming.
          returnBluetoothName = false
          receiverClass.receiverInfo(appID: appID, apiPort: rViewModel.apiPort, returnReceiverName: returnBluetoothName) {(bluetoothName: String?, isConnected: Int?) in
            if isConnected != nil && isConnected == 1 {
              self.isConnectedInt = isConnected ?? 0
              
              if isReceiverConnected {
                wsManager.disconnect()
              } else {
                wsManager.connect(with: rViewModel.locationV2Port)
              }
              isReceiverConnected.toggle()
//              Most consistent with text changes
            }
            else {
              wsManager.lat = "Please register your app"
              receiverClass.openReceiverSelectionScreen()
            }
          }
        }) {
          Text(isReceiverConnected ? "Stop Position Stream" : "Start Position Stream")
        }
        .buttonStyle(.borderedProminent)
      }
      .padding()
      
      Spacer()  // Spacer to push the content to the center
    }
    .padding()
  }
}

private func register(with appID: String) {
//  Sends the register URL to TMM with the required params. Should be the first button pressed.
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

#Preview {
  ContentView()
}
