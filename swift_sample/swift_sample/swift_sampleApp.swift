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
            ContentView()
        }
    }
}
