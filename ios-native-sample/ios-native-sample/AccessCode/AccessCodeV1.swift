import CryptoKit
import Foundation

enum AccessCodeV1 {
  private static func makeISO8601Formatter() -> ISO8601DateFormatter {
    let formatter = ISO8601DateFormatter()
    formatter.formatOptions = [.withInternetDateTime]
    formatter.timeZone = TimeZone(secondsFromGMT: 0)
    return formatter
  }

  static func generate(appId: UUID, utcTime: Date) -> String {
    let lowercaseId = appId.uuidString.lowercased()
    let iso8601Time = makeISO8601Formatter().string(from: utcTime)
    let plaintextAccessCode = lowercaseId + iso8601Time
    let hashed = SHA256.hash(data: Data(plaintextAccessCode.utf8))
    return Data(hashed).base64EncodedString()
  }
}
