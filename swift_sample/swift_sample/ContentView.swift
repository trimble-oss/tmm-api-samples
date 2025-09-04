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
  
  @State private var appID: String = UserDefaults.standard.string(forKey: "appID") ?? ""
  @State private var receiverName: String = ""
  
  // Registration - Allows the UI to receive the callback URL
  @EnvironmentObject var rViewModel: registrationViewModel
  
//  Receiver variables
  @State private var returnBluetoothName: Bool = true
  @State private var receiverClass = ReceiverClass()
  
  // WebSocket variables
  @State private var isConnectedInt: Int = 0
  @State private var isReceiverConnected: Bool = false
  @StateObject private var wsManager = WebSocketManager()
  
  var body: some View {
    VStack {
      // Header text fixed at the top
      Text(headerText)
        .font(.largeTitle)
        .padding([.top, .leading, .trailing], 20)
      
      Spacer()  // Spacer to push the content to bottom of the screen. Combined with the bottom Spacer() it centres the content.
     
      VStack {
        // Registration textfield and button
        HStack {
          TextField("Enter your app's ID", text: $appID)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .padding()
          
          Button("Register") {
//            function for handling registration URL
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
//            Shows the receiver's bluetooth name in the textfield
            returnBluetoothName = true
            if returnBluetoothName {
              receiverClass.receiverInfo(appID: appID, apiPort: rViewModel.apiPort, returnReceiverName: returnBluetoothName) { (bluetoothName: String?, isConnectedJson: Int?) in
                if bluetoothName != nil {
//                   bluetoothName comes from the completion handler in the function
                  self.receiverName = bluetoothName ?? ""
                }
              }
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
          returnBluetoothName = false
//          WebSocket position stream button.
//          Checks whether receiver is connected.
          receiverClass.receiverInfo(appID: appID, apiPort: rViewModel.apiPort, returnReceiverName: returnBluetoothName) {(bluetoothName: String?, isConnected: Int?) in
            if isConnected != nil && isConnected == 1 {
              self.isConnectedInt = isConnected ?? 0
              if isReceiverConnected {
//                Will disconnect from receiver if WebSocket is already connected.
                wsManager.disconnect()
              } else {
//                Connects to WebSocket if receiver is connected.
                wsManager.connect(with: rViewModel.locationV2Port)
              }
//             Most consistent with text changes. Changes the state of receiver connected status.
              isReceiverConnected.toggle()
            }
            else {
//              Pop up window asking user whether they want to configure the receiver.
//              This is triggered if app is registered but receiver is not connected.
              let alert = UIAlertController(title: "DA2 receiver not connected or app not registered", message: "Press cancel if your app hasn't been registered.\nOtherwise click either connect receiver or configure receiver", preferredStyle: .alert)
              let configureAction = 	UIAlertAction(title: "Configure receiver", style: .default) {_ in
//                Opens configuration screen in TMM
                receiverClass.openTmmScreen(url: "tmmopentoconfiguration://?")
              }
              
              let receiverSelectAction = UIAlertAction(title: "Connect receiver", style: .default) {_ in
//                Opens receiver selection screen in TMM
                receiverClass.openTmmScreen(url: "tmmopentoreceiverselection://?")
              }
              
//              Cancel button
              let cancelAction = UIAlertAction(title: "Cancel", style: .cancel, handler: nil)
              
              alert.addAction(configureAction)
              alert.addAction(receiverSelectAction)
              alert.addAction(cancelAction)
              
              if let windowScene = UIApplication.shared.connectedScenes.first as? UIWindowScene,
                 let rootViewController = windowScene.windows.first?.rootViewController {
                rootViewController.present(alert, animated: true, completion: nil)
              }
            }
          }
        }) {
          Text(isReceiverConnected ? "Stop Position Stream" : "Start Position Stream")
//          Changes button text based on receiver connection status
        }
        .buttonStyle(.borderedProminent)
      }
      .padding()
      
      Spacer()  // Spacer to push the content upwards. Combined with the top Spacer() it centres the screen
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
    // save the appID for later
    UserDefaults.standard.set(appID, forKey: "appID")
    
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
