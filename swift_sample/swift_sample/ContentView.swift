//
//  ContentView.swift
//  swift_sample
//
//  Created by Terence Chen on 14/01/2025.
//

import SwiftUI

struct ContentView: View {
  @EnvironmentObject var appState: AppState
  @State private var appID: String = ""
  @State private var receiverName: String = ""
  @State private var lat: String = ""
  @State private var long: String = ""
  @State private var alt: String = ""
  // All need a value
  
  private var headerText: String {
    "Swift Sample: Version \(versionNumber)"
  }
  private var versionNumber: String {
          Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String ?? "Unknown"
      }
  
  var body: some View {
      VStack {
        // Header text fixed at the top
        Text(headerText)
          .font(.largeTitle)
          .padding([.top, .leading, .trailing], 20)
        
        Spacer() // Spacer to push the content to the center
        
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
            Button("Get Receiver") {
              // Change activity
            }
            .buttonStyle(.borderedProminent)
          }
          
          
          // WebSocket
          TextField("Latitude", text: $lat)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .disabled(true)
            .padding()
          TextField("Longitude", text: $long)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .disabled(true)
            .padding()
          TextField("Altitude", text: $alt)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .disabled(true)
            .padding()
          
          Button("Start Position Stream") {
            // Change activity
          }
          .buttonStyle(.borderedProminent)
        }
        .padding()
        
        Spacer() // Spacer to push the content to the center
      }
    .padding()
  }
}

private func register(with appID: String) {
  print("Registering with app ID: \(appID)")
  //  returl
  var params: [String: String] =
  [
  "application_id": appID,
  "returl": "trimble.tmm.iOS"
  ]

  do {
    let jsonData = try JSONSerialization.data(withJSONObject: params, options: [])
    let jsonString = String(data: jsonData, encoding: .utf8)
    let base64Encoded = jsonString?.data(using: .utf8)?.base64EncodedString()
    let customUrl = URL(string: "tmmregister://?" + base64Encoded!)!
    UIApplication.shared.open(customUrl) { (success) in
      if success{
      }
    }
  }
  catch {
    print("Json serialization issue: \(error.localizedDescription)")
  }
}

#Preview {
    ContentView()
}
