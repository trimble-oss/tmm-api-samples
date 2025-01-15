//
//  AppDelegate.swift
//  swift_sample
//
//  Created by kea-build on 14/01/2025.
//

import UIKit

struct RegistrationResult: Codable {
    let registrationResult: String
    let apiPort: Int
    let locationPort: Int
    let locationSecurePort: Int
    let locationV2Port: Int
}

class AppDelegate: UIResponder, UIApplicationDelegate {

    var window: UIWindow?

    func application(_ app: UIApplication, open url: URL, options: [UIApplication.OpenURLOptionsKey : Any] = [:]) -> Bool {
        // Handle the callback URL here
        if let scheme = url.scheme {
            if let base64String = url.query {
                handleBase64EncodedData(base64String: base64String)
            }
            return true
        }
        return false
    }

    func handleBase64EncodedData(base64String: String) {
        // Decode the base64 encoded string
        if let jsonData = Data(base64Encoded: base64String) {
            let decoder = JSONDecoder()
            do {
                let result = try decoder.decode(RegistrationResult.self, from: jsonData)
                // Print the decoded data
                print("Registration Result: \(result.registrationResult)")
                print("API Port: \(result.apiPort)")
                print("Location Port: \(result.locationPort)")
                print("Location Secure Port: \(result.locationSecurePort)")
                print("Location V2 Port: \(result.locationV2Port)")
                // Add your custom handling logic here if needed
            } catch {
                print("Failed to decode JSON: \(error)")
            }
        }
    }

    // Other AppDelegate methods...
}
