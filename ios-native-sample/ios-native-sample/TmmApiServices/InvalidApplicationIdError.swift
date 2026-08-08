import Foundation

struct InvalidApplicationIdError: Error, LocalizedError {
  let appId: String

  var errorDescription: String? {
    "Invalid App ID \"\(appId)\""
  }
}
