//
//  AppDelegate.swift
//  swift_sample
//
//  Created by kea-build on 14/01/2025.
//

import UIKit
import SwiftUI

class AppDelegate: UIResponder, UIApplicationDelegate {
  
  var window: UIWindow?
  var viewModel = registrationViewModel()
  
  func application(_ application: UIApplication,
                   open url: URL,
                   options: [UIApplication.OpenURLOptionsKey : Any] = [:] ) -> Bool {
    
    // Changed from what is in the docs for iOS version 15 but functions the same
    if let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
       let query = components.query,
       let data = Data(base64Encoded: query) {
      do {
        if let jsonObject = try JSONSerialization.jsonObject(with: data, options: []) as? [String: String] {
          viewModel.registrationResult = jsonObject["registrationResult"]!
          viewModel.locationPort = Int(jsonObject["locationPort"]!)
          viewModel.locationV2Port = Int(jsonObject["locationV2Port"]!)
          viewModel.apiPort = Int(jsonObject["apiPort"]!)
          return true
        }
      } catch {
        print("Error deserializing JSON: \(error)")
        return false
      }
    } else {
      print("Error: Unable to extract query from URL or decode data")
    }
    return false
  }
}
