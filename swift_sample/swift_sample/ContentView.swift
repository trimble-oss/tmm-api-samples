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
  
  // All need a value
  @State private var appID: String = ""
  @State private var receiverName: String = ""
  
  // Registration
  @EnvironmentObject var rViewModel: registrationViewModel
  
//  Receiver
  @State private var returnBluetoothName: Bool = true
  @State private var receiverClass = ReceiverClass()
  
  // Web socket location
  @State private var isConnectedInt: Int = 0
  @State private var isConnectedBool: Bool = false
  @StateObject private var wsManager = WebSocketManager()
  
  var body: some View {
    VStack {
      // Header text fixed at the top
      Text(headerText)
        .font(.largeTitle)
        .padding([.top, .leading, .trailing], 20)
      
      Spacer()  // Spacer to push the content to the center
     
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
        
        // REST API - Receiver
        HStack {
          TextField("Receiver name", text: $receiverName)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .disabled(true)
            .padding()
          Button("Get Receiver", action: {
            returnBluetoothName = true
            if returnBluetoothName {
              receiverClass.receiverInfo(appID: appID, apiPort: rViewModel.apiPort, bluetoothNameBool: returnBluetoothName) { (bluetoothName: String?, isConnectedJson: Int?) in
                if bluetoothName != nil {
                  self.receiverName = bluetoothName ?? ""
                }
              }
//              bluetoothName comes from the completion handler in the function
            }
          })
        }
        .buttonStyle(.borderedProminent)
        
        // WebSocket
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
          returnBluetoothName = false
          receiverClass.receiverInfo(appID: appID, apiPort: rViewModel.apiPort, bluetoothNameBool: returnBluetoothName) {(bluetoothName: String?, isConnected: Int?) in
            if isConnected != nil && isConnected == 1 {
              self.isConnectedInt = isConnected ?? 0
              
              if isConnectedBool {
                wsManager.disconnect()
              } else {
                wsManager.connect(with: rViewModel.locationV2Port)
              }
              isConnectedBool.toggle()
            }
            else {
              wsManager.lat = "Please register your app"
              receiverClass.openReceiverSelectionScreen()
            }
          }
        }) {
          Text(isConnectedBool ? "Stop Position Stream" : "Start Position Stream")
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
  let params: [String: String] =
  [
    "application_id": appID,
    "returl": "tmmapisample://com.trimble.tmmapisample",
  ]
//    Create a custom url scheme for your app in the info.plist where "tmmapisample" is the URL schemes if you're using the GUI in Xcode and "com.trimble.tmmapisample" is the identifier.
//    If you're working with the source code, then it will look something like this:
//    <dict>
//      <key>CFBundleURLTypes</key>
//      <array>
//        <dict>
//          <key>CFBundleTypeRole</key>
//          <string>None</string>
//          <key>CFBundleURLName</key>
//          <string>com.trimble.tmmapisample</string>
//          <key>CFBundleURLSchemes</key>
//          <array>
//            <string>tmmapisample</string>
//          </array>
//        </dict>
//      </array>
//    </dict>
  
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
