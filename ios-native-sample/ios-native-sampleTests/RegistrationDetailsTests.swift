import Foundation
import Testing
@testable import ios_native_sample

struct RegistrationDetailsTests {
  @Test func parse_extractsAllPortsFromCallbackUrl() throws {
    let payload: [String: Any] = [
      "registrationResult": "OK",
      "locationPort": 9635,
      "locationSecurePort": 9636,
      "apiPort": 9637,
      "apiSecurePort": 9638,
      "locationV2Port": 9639,
      "locationV2SecurePort": 9640,
    ]
    let jsonData = try JSONSerialization.data(withJSONObject: payload)
    let base64 = jsonData.base64EncodedString()
    let url = try #require(URL(string: "tmmapiossample://response/register?\(base64)"))

    let details = try #require(RegistrationDetails.parse(from: url))

    #expect(details.registrationResult == "OK")
    #expect(details.isSuccess)
    #expect(details.locationPort == 9635)
    #expect(details.locationSecurePort == 9636)
    #expect(details.apiPort == 9637)
    #expect(details.apiSecurePort == 9638)
    #expect(details.locationV2Port == 9639)
    #expect(details.locationV2SecurePort == 9640)
  }
}
