import Foundation

struct RegistrationDetails {
  var registrationResult: String = ""
  var locationPort: Int = 0
  var locationSecurePort: Int = 0
  var apiPort: Int = 0
  var apiSecurePort: Int = 0
  var locationV2Port: Int = 0
  var locationV2SecurePort: Int = 0

  var isSuccess: Bool {
    registrationResult.caseInsensitiveCompare("OK") == .orderedSame
      || registrationResult.caseInsensitiveCompare("success") == .orderedSame
  }

  static func parse(from url: URL) -> RegistrationDetails? {
    guard let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
          let query = components.query,
          let data = Data(base64Encoded: query),
          let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any]
    else {
      return nil
    }

    var details = RegistrationDetails()
    details.registrationResult = json["registrationResult"] as? String ?? ""
    details.locationPort = parsePort(json, key: "locationPort")
    details.locationSecurePort = parsePort(json, key: "locationSecurePort")
    details.apiPort = parsePort(json, key: "apiPort")
    details.apiSecurePort = parsePort(json, key: "apiSecurePort")
    details.locationV2Port = parsePort(json, key: "locationV2Port")
    details.locationV2SecurePort = parsePort(json, key: "locationV2SecurePort")
    return details
  }

  private static func parsePort(_ json: [String: Any], key: String) -> Int {
    if let value = json[key] as? Int {
      return value
    }
    if let value = json[key] as? String, let port = Int(value) {
      return port
    }
    return 0
  }
}
