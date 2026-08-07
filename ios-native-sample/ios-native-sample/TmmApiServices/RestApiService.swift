import Foundation

enum RestApiService {
  private static let host = "tmm-api-local.fieldsystems.trimble.com"

  /// Defaults to V2. Change to `.v1` to use Basic auth instead.
  nonisolated(unsafe) static var accessCodeVersion: AccessCodeVersion = .v2

  private static let session: URLSession = {
    let configuration = URLSessionConfiguration.default
    configuration.timeoutIntervalForRequest = 60
    configuration.timeoutIntervalForResource = 60
    return URLSession(configuration: configuration)
  }()

  private static var baseURL: URL {
    URL(string: "https://\(host):\(PortInfo.apiSecurePort)/")!
  }

  static func getPublicKey() async -> String? {
    let url = baseURL.appendingPathComponent("api/v1/publicKey")
    var request = URLRequest(url: url)
    request.httpMethod = "GET"
    request.timeoutInterval = 1

    do {
      let (data, response) = try await session.data(for: request)
      guard let httpResponse = response as? HTTPURLResponse,
            (200 ... 299).contains(httpResponse.statusCode)
      else {
        return nil
      }
      return String(data: data, encoding: .utf8)
    } catch {
      return nil
    }
  }

  static func getReceiver(applicationId: String) async throws -> ReceiverInfo? {
    let url = baseURL.appendingPathComponent("api/v1/receiver")
    var request = URLRequest(url: url)
    request.httpMethod = "GET"
    request.setValue(try authorizationHeader(applicationId: applicationId), forHTTPHeaderField: "Authorization")

    let (data, response) = try await session.data(for: request)
    guard let httpResponse = response as? HTTPURLResponse,
          (200 ... 299).contains(httpResponse.statusCode)
    else {
      return nil
    }

    let decoder = JSONDecoder()
    decoder.keyDecodingStrategy = .convertFromSnakeCase
    return try decoder.decode(ReceiverInfo.self, from: data)
  }

  static func putReceiver(applicationId: String, isConnected: Bool) async throws {
    let url = baseURL.appendingPathComponent("api/v1/receiver")
    var request = URLRequest(url: url)
    request.httpMethod = "PUT"
    request.setValue("application/json", forHTTPHeaderField: "Content-Type")
    request.setValue(try authorizationHeader(applicationId: applicationId), forHTTPHeaderField: "Authorization")
    request.httpBody = try JSONSerialization.data(withJSONObject: ["isConnected": isConnected])

    let (_, response) = try await session.data(for: request)
    if let httpResponse = response as? HTTPURLResponse,
       !(200 ... 299).contains(httpResponse.statusCode)
    {
      // Matches MAUI/Android: log but do not throw on PUT failure.
    }
  }

  private static func authorizationHeader(applicationId: String) throws -> String {
    let trimmed = applicationId.trimmingCharacters(in: .whitespacesAndNewlines)
    guard let appId = UUID(uuidString: trimmed) else {
      throw InvalidApplicationIdError(appId: applicationId)
    }

    let now = Date()
    switch accessCodeVersion {
    case .v1:
      return "Basic \(AccessCodeV1.generate(appId: appId, utcTime: now))"
    case .v2:
      return "AccessCodeV2 \(try AccessCodeV2.generate(appId: appId, utcTime: now))"
    }
  }
}
