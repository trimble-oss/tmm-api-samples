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
  var viewModel: ViewModel?
  
  func application(_ app: UIApplication, open url: URL, options: [UIApplication.OpenURLOptionsKey : Any] = [:]) -> Bool {
    guard let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
          let queryItems = components.queryItems else {
      return false
    }
    
    for queryItem in queryItems {
      switch queryItem.name {
      case "registrationResult":
        viewModel?.registrationResult = queryItem.value
      case "apiPort":
        viewModel?.apiPort = Int(queryItem.value ?? "")
      case "locationPort":
        viewModel?.locationPort = Int(queryItem.value ?? "")
      case "locationSecurePort":
        viewModel?.locationSecurePort = Int(queryItem.value ?? "")
      case "locationV2Port":
        viewModel?.locationV2Port = Int(queryItem.value ?? "")
      default:
        break
      }
    }
    return true
  }
}
