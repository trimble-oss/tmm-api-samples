//
//  ContentView.swift
//  swift_sample
//
//  Created by Terence Chen on 14/01/2025.
//

import SwiftUI

struct ContentView: View {
  @State private var appID: String = ""
  @State private var receiverName: String = ""
  @State private var webSocketData: String = "Waiting for message..."
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
          .padding(.top, 20)
        
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
              .padding()
            Button("Get Receiver") {
              // Change activity
            }
            .buttonStyle(.borderedProminent)
          }
          
          
          // WebSocket
          TextField("Latitude", text: $lat)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .padding()
          TextField("Longitude", text: $long)
            .textFieldStyle(RoundedBorderTextFieldStyle())
            .padding()
          TextField("Altitude", text: $alt)
            .textFieldStyle(RoundedBorderTextFieldStyle())
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
    }

#Preview {
    ContentView()
}
