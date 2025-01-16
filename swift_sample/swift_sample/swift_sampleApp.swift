//
//  swift_sampleApp.swift
//  swift_sample
//
//  Created by kea-build on 14/01/2025.
//

import SwiftUI

@main
struct swift_sampleApp: App {
  @UIApplicationDelegateAdaptor(AppDelegate.self) var appDelegate
  
  var body: some Scene {
    WindowGroup {
      let viewModel = ViewModel()
      appDelegate.viewModel = viewModel
      ContentView()
        .environmentObject(viewModel)
        .onOpenURL{ url in
          handleIncomingURL(url, viewModel: viewModel)
      }
    }
  }
}

private func handleIncomingURL(_ url: URL, viewModel: ViewModel) {
  guard let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
        let queryItems = components.queryItems else {
    return
  }
  
  var registrationResult: String?
  var apiPort: Int?
  var locationPort: Int?
  var locationSecurePort: Int?
  var locationV2Port: Int?
  
  for queryItem in queryItems {
    switch queryItem.name {
    case "registrationResult":
      registrationResult = queryItem.value
    case "apiPort":
      apiPort = Int(queryItem.value ?? "")
    case "locationPort":
      locationPort = Int(queryItem.value ?? "")
    case "locationSecurePort":
      locationSecurePort = Int(queryItem.value ?? "")
    case "locationV2Port":
      locationV2Port = Int(queryItem.value ?? "")
    default:
      break
    }
  }
  
  // Handle the callback data as needed
  if let result = registrationResult {
    switch result {
    case "OK":
      print("Registration OK")
    case "NoNetwork":
      print("No Network")
    case "Unauthorized":
      print("Unauthorized")
    default:
      break
    }
  }
  
  if let apiPort = apiPort {
    print("API Port: \(apiPort)")
  }
  
  if let locationPort = locationPort {
    print("Location Port: \(locationPort)")
  }
  
  if let locationSecurePort = locationSecurePort {
    print("Location Secure Port: \(locationSecurePort)")
  }
  
  if let locationV2Port = locationV2Port {
    print("Location V2 Port: \(locationV2Port)")
  }
}
