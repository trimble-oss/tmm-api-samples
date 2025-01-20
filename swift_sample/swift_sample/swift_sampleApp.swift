//
//  swift_sampleApp.swift
//  swift_sample
//
//  Created by kea-build on 14/01/2025.
//

import SwiftUI

@main
struct swift_sampleApp: App {
  @State private var rViewModel = registrationViewModel()
  
  var body: some Scene {
    WindowGroup {
      ContentView()
        .environmentObject(rViewModel)
//      Keeps the ContentView updated when view model values are changed
        .onOpenURL { url in
          handleCallbackURL(url)
        }
    }
  }
  
  private func handleCallbackURL(_ url: URL) {
    // URL returns json. Extract the properties from the json
    if let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
       let query = components.query,
       let data = Data(base64Encoded: query) {
      do {
        if let jsonObject = try JSONSerialization.jsonObject(with: data, options: []) as? [String: String] {
          rViewModel.registrationResult = jsonObject["registrationResult"]!
          rViewModel.apiPort = Int(jsonObject["apiPort"]!)!
          rViewModel.locationV2Port = Int(jsonObject["locationV2Port"]!)!
          
//          Updates the view model with parsed json
        }
      } catch {
        print("Error deserializing JSON: \(error)")
      }
    } else {
      print("Error: Unable to extract query from URL or decode data")
    }
  }
}
