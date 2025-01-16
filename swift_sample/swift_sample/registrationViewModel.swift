//
//  registrationViewModel.swift
//  swift_sample
//
//  Created by kea-build on 16/01/2025.
//

import SwiftUI
import Combine

class registrationViewModel: ObservableObject {
    @Published var registrationResult: String?
    @Published var apiPort: Int?
    @Published var locationPort: Int?
    @Published var locationSecurePort: Int?
    @Published var locationV2Port: Int?
}
