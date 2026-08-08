import Foundation
import Testing
@testable import ios_native_sample

struct AccessCodeV1Tests {
  private let appId = UUID(uuidString: "12345678-1234-1234-1234-123456789abc")!
  private let utcTime = ISO8601DateFormatter().date(from: "2024-02-22T18:00:00Z")!

  @Test func generate_isDeterministicForSameInputs() {
    let first = AccessCodeV1.generate(appId: appId, utcTime: utcTime)
    let second = AccessCodeV1.generate(appId: appId, utcTime: utcTime)

    #expect(first == second)
    #expect(first.count == 44)
  }

  @Test func generate_changesWithTime() {
    let firstTime = ISO8601DateFormatter().date(from: "2024-02-22T18:00:00Z")!
    let secondTime = ISO8601DateFormatter().date(from: "2024-02-22T18:00:01Z")!

    let first = AccessCodeV1.generate(appId: appId, utcTime: firstTime)
    let second = AccessCodeV1.generate(appId: appId, utcTime: secondTime)

    #expect(first != second)
  }
}
