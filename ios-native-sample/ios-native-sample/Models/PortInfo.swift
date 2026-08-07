import Foundation

enum PortInfo: Sendable {
  nonisolated(unsafe) static var locationPort = 9635
  nonisolated(unsafe) static var locationSecurePort = 9636
  nonisolated(unsafe) static var apiPort = 9637
  nonisolated(unsafe) static var apiSecurePort = 9638
  nonisolated(unsafe) static var locationV2Port = 9639
  nonisolated(unsafe) static var locationV2SecurePort = 9640

  static func apply(registration: RegistrationDetails) {
    locationPort = registration.locationPort
    locationSecurePort = registration.locationSecurePort
    apiPort = registration.apiPort
    apiSecurePort = registration.apiSecurePort
    locationV2Port = registration.locationV2Port
    locationV2SecurePort = registration.locationV2SecurePort
  }
}
