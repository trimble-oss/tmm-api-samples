//
//  AppDelegate.swift
//  swift_sample
//
//  Created by Terence Chen on 14/01/2025.
//

import UIKit

class AppState: ObservableObject {
    @Published var registrationResult: String?
    @Published var locationPort: Int?
    @Published var locationV2Port: Int?
    @Published var apiPort: Int?
}

@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {
  
  var window: UIWindow?
  var appState = AppState()
  
  
  func application(_ application: UIApplication,
                   open url: URL,
                   options: [UIApplication.OpenURLOptionsKey : Any] = [:] ) -> Bool {
    
    do {
      let query = url.query()!
      let data = Data(base64Encoded: query)!
      let jsonObject = try JSONSerialization.jsonObject(with: data, options: [])
      let results = (jsonObject as? [String: String])!
      
      // registrationResult:
      // OK: Your app is registered
      // Unauthorized: TMM was not able to verify the Application ID
      // NoNetwork: There is not internet connection
      let registrationResult = results["registrationResult"]!
      // localhost port for WebSocket Locations, Version 2
      let locationV2Port = Int(results["locationV2Port"]!)
      // The REST API is at $"WS://localhost:{apiPort}"
      let apiPort = Int(results["apiPort"]!)
      
      return true;
    } catch {
      print(error.localizedDescription)
      return false;
    }
  }
}
