import Foundation
import UIKit

@MainActor
final class PlatformRequestService {
  static let shared = PlatformRequestService()

  private static let registerReturnUrl = "tmmapiossample://response/register"

  private var registrationContinuation: CheckedContinuation<RegistrationDetails?, Never>?

  private init() {}

  func register(applicationId: String) async -> RegistrationDetails? {
    let payload: [String: String] = [
      "application_id": applicationId,
      "returl": Self.registerReturnUrl,
    ]

    guard let jsonData = try? JSONSerialization.data(withJSONObject: payload),
          let jsonString = String(data: jsonData, encoding: .utf8),
          let base64Payload = jsonString.data(using: .utf8)?.base64EncodedString(),
          let url = URL(string: "tmmregister://?\(base64Payload)")
    else {
      return nil
    }

    return await withCheckedContinuation { continuation in
      registrationContinuation = continuation
      UIApplication.shared.open(url, options: [:]) { success in
        if !success {
          continuation.resume(returning: nil)
          self.registrationContinuation = nil
        }
      }
    }
  }

  func showReceiverSelection() {
    guard let url = URL(string: "tmmopentoreceiverselection://?") else {
      return
    }
    UIApplication.shared.open(url)
  }

  func handleIncomingURL(_ url: URL) {
    guard url.absoluteString.hasPrefix(Self.registerReturnUrl),
          let details = RegistrationDetails.parse(from: url)
    else {
      return
    }

    registrationContinuation?.resume(returning: details)
    registrationContinuation = nil
  }
}
