import Foundation
import Security

enum AccessCodeV2 {
  nonisolated(unsafe) private static var publicKey: SecKey?

  private static func makeISO8601Formatter() -> ISO8601DateFormatter {
    let formatter = ISO8601DateFormatter()
    formatter.formatOptions = [.withInternetDateTime]
    formatter.timeZone = TimeZone(secondsFromGMT: 0)
    return formatter
  }

  static func setPublicKey(_ jwkJson: String) throws {
    guard !jwkJson.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
      throw AccessCodeV2Error.invalidJwk("JWK JSON must not be blank.")
    }

    guard let data = jwkJson.data(using: .utf8),
          let json = try JSONSerialization.jsonObject(with: data) as? [String: Any]
    else {
      throw AccessCodeV2Error.invalidJwk("Invalid JWK JSON.")
    }

    guard json["kty"] as? String == "RSA" else {
      throw AccessCodeV2Error.invalidJwk("Only RSA JWK keys are supported.")
    }

    guard let modulusString = json["n"] as? String,
          let exponentString = json["e"] as? String
    else {
      throw AccessCodeV2Error.invalidJwk("n and e properties are required.")
    }

    let modulus = try base64UrlDecode(modulusString)
    let exponent = try base64UrlDecode(exponentString)

    var error: Unmanaged<CFError>?
    let attributes: [String: Any] = [
      kSecAttrKeyType as String: kSecAttrKeyTypeRSA,
      kSecAttrKeyClass as String: kSecAttrKeyClassPublic,
    ]

    guard let key = SecKeyCreateWithData(
      rsaPublicKeyData(modulus: modulus, exponent: exponent) as CFData,
      attributes as CFDictionary,
      &error
    ) else {
      throw error?.takeRetainedValue() ?? AccessCodeV2Error.invalidJwk("Failed to create RSA public key.")
    }

    publicKey = key
  }

  static func generate(appId: UUID, utcTime: Date) throws -> String {
    guard let key = publicKey else {
      throw AccessCodeV2Error.publicKeyNotSet
    }

    let lowercaseId = appId.uuidString.lowercased()
    let iso8601Time = makeISO8601Formatter().string(from: utcTime)
    let plaintextAccessCode = "\(lowercaseId) \(iso8601Time)"
    let plaintextData = Data(plaintextAccessCode.utf8)

    var error: Unmanaged<CFError>?
    guard let encrypted = SecKeyCreateEncryptedData(
      key,
      .rsaEncryptionOAEPSHA256,
      plaintextData as CFData,
      &error
    ) as Data? else {
      throw error?.takeRetainedValue() ?? AccessCodeV2Error.encryptionFailed
    }

    return encrypted.base64EncodedString()
  }

  private static func base64UrlDecode(_ base64Url: String) throws -> Data {
    var base64 = base64Url
      .replacingOccurrences(of: "-", with: "+")
      .replacingOccurrences(of: "_", with: "/")
    let padding = (4 - base64.count % 4) % 4
    base64 += String(repeating: "=", count: padding)
    guard let data = Data(base64Encoded: base64) else {
      throw AccessCodeV2Error.invalidJwk("Invalid base64url encoding.")
    }
    return data
  }

  /// Builds DER-encoded RSAPublicKey (PKCS#1) for SecKeyCreateWithData.
  private static func rsaPublicKeyData(modulus: Data, exponent: Data) -> Data {
    let modulusEncoded = derInteger(modulus)
    let exponentEncoded = derInteger(exponent)
    let sequenceBody = modulusEncoded + exponentEncoded
    return derSequence(sequenceBody)
  }

  private static func derInteger(_ value: Data) -> Data {
    var bytes = value
    if bytes.first == 0x00 || (bytes.first ?? 0) >= 0x80 {
      bytes = Data([0x00]) + bytes
    }
    return Data([0x02]) + derLength(bytes.count) + bytes
  }

  private static func derSequence(_ body: Data) -> Data {
    Data([0x30]) + derLength(body.count) + body
  }

  private static func derLength(_ length: Int) -> Data {
    if length < 128 {
      return Data([UInt8(length)])
    }
    var value = length
    var bytes: [UInt8] = []
    while value > 0 {
      bytes.insert(UInt8(value & 0xFF), at: 0)
      value >>= 8
    }
    return Data([UInt8(0x80 | bytes.count)] + bytes)
  }
}

enum AccessCodeV2Error: Error, LocalizedError {
  case publicKeyNotSet
  case encryptionFailed
  case invalidJwk(String)

  var errorDescription: String? {
    switch self {
    case .publicKeyNotSet:
      return "Public key not set."
    case .encryptionFailed:
      return "RSA encryption failed."
    case .invalidJwk(let message):
      return message
    }
  }
}
