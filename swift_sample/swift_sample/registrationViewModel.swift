//
//  registrationViewModel.swift
//  swift_sample
//
//  Created by kea-build on 16/01/2025.
//

import SwiftUI
import Combine

class registrationViewModel: ObservableObject {
    @Published var registrationResult: String = ""
    @Published var apiPort: Int = -1
    @Published var locationPort: Int = -1
    @Published var locationSecurePort: Int = -1
    @Published var locationV2Port: Int = -1
}
