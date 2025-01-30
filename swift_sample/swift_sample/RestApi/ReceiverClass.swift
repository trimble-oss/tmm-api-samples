//
//  ReceiverClass.swift
//  swift_sample
//
//  Created by kea_build on 22/01/2025.
//
import Foundation
import SwiftUI

class ReceiverClass {
  
  func openReceiverSelectionScreen() {
  //  If no input is needed from the schemas then simply inputting the URL without the params array is fine
      if let customUrl = URL(string: "tmmopentoreceiverselection://?") {
        UIApplication.shared.open(customUrl, options: [:]) { success in
          if success {
            print("The URL was delivered successfully.")
          } else {
            print("Failed to open the URL.")
          }
        }
      }
  }
  
  func receiverInfo(appID: String, apiPort: Int, bluetoothNameBool: Bool, completion: @escaping (String?, Int?) -> Void) {
//    This function will return 2 variables, bluetoothName(String) and isConnected(Int) so that it can be reused in the View depending on what is returned.
    guard let url = URL(string: "http://localhost:\(apiPort)/api/v1/receiver") else {
      print("Invalid URL")
      return
    }
    if apiPort == -1 {
      DispatchQueue.main.async {
//        Updates to UI's must be completed on the main thread
        completion("Invalid api port or App is not registered", nil)
      }
    }
    
    let utcTime = Date()
  //  Generates the access code
    guard let accessCode = AccessCodeGenerator.generateAccessCode(appID: appID, utcTime: utcTime) else {
      print("Failed to generate access code")
      return
    }
    
    var request = URLRequest(url: url)
    request.httpMethod = "GET"
    request.addValue("Basic \(accessCode)", forHTTPHeaderField: "Authorization")
    
    let task = URLSession.shared.dataTask(with: request) { data, response, error in
      if let error = error {
        print("\(error.localizedDescription) App might not be registered.")
        return
      }
      
      guard let data = data else {
        print("No data received")
        return
      }
      
      do {
        if let json = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any] {
          let bluetoothName = json["bluetoothName"] as? String ?? "App is not registered"
          let isConnected = json["isConnected"] as? Int ?? 0
          DispatchQueue.main.async {
  //          Updates to UI's must be completed on the main thread
            if bluetoothNameBool {
//              This variable determines if the View wants the bluetoothName or not
              completion(bluetoothName, nil)
            } else {
              completion(nil, isConnected)
            }
          }
        } else {
          print("Invalid JSON format or app is not registered")
        }
      } catch {
        print("JSON parsing error: \(error.localizedDescription)")
      }
    }
    task.resume()
  //  Starts connection
  }
}
