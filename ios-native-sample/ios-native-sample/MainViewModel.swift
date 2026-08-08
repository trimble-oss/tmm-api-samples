import Foundation
import Observation

enum MainUiEvent: Equatable {
  case alert(title: String, message: String)
  case confirmReceiverNotConfigured(message: String)
}

@Observable
@MainActor
final class MainViewModel {
  var applicationId: String {
    didSet {
      appPreferences.appId = applicationId
    }
  }

  var statusMessage = ""
  var latitudeText = ""
  var longitudeText = ""
  var altitudeText = ""
  var accuracyText = ""
  private(set) var isConnecting = false
  private(set) var isConnected = false

  var canConnect: Bool { !isConnecting && !isConnected }
  var canDisconnect: Bool { !isConnecting && isConnected }

  private let appPreferences: AppPreferences
  private let platformRequestService: PlatformRequestService
  private let webSocketService: WebSocketService
  private var positionTask: Task<Void, Never>?

  var pendingEvent: MainUiEvent?

  init(
    appPreferences: AppPreferences = .shared,
    platformRequestService: PlatformRequestService = .shared,
    webSocketService: WebSocketService = WebSocketService()
  ) {
    self.appPreferences = appPreferences
    self.platformRequestService = platformRequestService
    self.webSocketService = webSocketService
    applicationId = appPreferences.appId
  }

  func connect() async {
    guard canConnect else { return }

    if appPreferences.appId.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty {
      statusMessage = "Please enter an Application ID."
      pendingEvent = .alert(title: "Error", message: "Please enter an Application ID")
      return
    }

    setConnectionState(isConnecting: true, isConnected: false)
    statusMessage = "Connecting..."

    do {
      statusMessage = "Getting public key..."
      var publicKey = await tryGetPublicKey()
      if publicKey == nil {
        statusMessage = "Registering..."
        if !(await tryRegister()) {
          statusMessage = "Registration failed."
          return
        }

        statusMessage = "Getting public key again..."
        publicKey = await tryGetPublicKey()
        if publicKey == nil {
          statusMessage = "Failed to get public key."
          return
        }
      }

      statusMessage = "Getting receiver info..."
      var receiver = try await RestApiService.getReceiver(applicationId: appPreferences.appId)
      if receiver == nil {
        statusMessage = "Failed to get receiver info."
        return
      }

      if receiver?.isReceiverConfigured != true {
        statusMessage = "Receiver not configured. Select a receiver in TMM."
        pendingEvent = .confirmReceiverNotConfigured(
          message: "Connect to a receiver in TMM to start streaming positions."
        )
        return
      }

      if receiver?.isConnected != true {
        statusMessage = "Connecting to GNSS receiver..."
        try await RestApiService.putReceiver(applicationId: appPreferences.appId, isConnected: true)
        receiver = try await RestApiService.getReceiver(applicationId: appPreferences.appId)
        if receiver == nil || receiver?.isConnected != true {
          statusMessage = "Failed to connect to GNSS receiver."
          return
        }
      }

      statusMessage = "Starting position stream..."
      startPositionStream()
      statusMessage = "Connected."
      setConnectionState(isConnecting: false, isConnected: true)
    } catch let error as InvalidApplicationIdError {
      statusMessage = "Invalid Application ID."
      pendingEvent = .alert(title: "Error", message: error.localizedDescription)
      setConnectionState(isConnecting: false, isConnected: false)
    } catch {
      statusMessage = "Connection failed."
      pendingEvent = .alert(
        title: "Error",
        message: "An unexpected error occurred while connecting."
      )
      setConnectionState(isConnecting: false, isConnected: false)
    }

    if isConnecting {
      setConnectionState(isConnecting: false, isConnected: false)
    }
  }

  func disconnect() {
    guard canDisconnect || isConnected else { return }

    setConnectionState(isConnecting: true, isConnected: false)
    statusMessage = "Disconnecting..."

    positionTask?.cancel()
    positionTask = nil
    webSocketService.stop()
    clearLocationData()

    statusMessage = "Disconnected."
    setConnectionState(isConnecting: false, isConnected: false)
  }

  func openReceiverSelection() {
    platformRequestService.showReceiverSelection()
  }

  func clearPendingEvent() {
    pendingEvent = nil
  }

  private func tryGetPublicKey() async -> String? {
    do {
      let publicKey = await RestApiService.getPublicKey()
      if let publicKey {
        try AccessCodeV2.setPublicKey(publicKey)
      }
      return publicKey
    } catch {
      return nil
    }
  }

  private func tryRegister() async -> Bool {
    let details = await platformRequestService.register(applicationId: appPreferences.appId)
    guard let details, !details.registrationResult.isEmpty else {
      return false
    }

    if details.registrationResult.caseInsensitiveCompare("OK") == .orderedSame {
      PortInfo.apply(registration: details)
    }

    return details.isSuccess
  }

  private func startPositionStream() {
    positionTask?.cancel()
    positionTask = Task {
      for await position in webSocketService.readPositions() {
        guard !Task.isCancelled else { break }
        latitudeText = Self.formatCoordinate(position.latitude)
        longitudeText = Self.formatCoordinate(position.longitude)
        altitudeText = Self.formatDistance(position.altitude)
        accuracyText = Self.formatDistance(position.hrms)
      }
    }
  }

  private func clearLocationData() {
    latitudeText = ""
    longitudeText = ""
    altitudeText = ""
    accuracyText = ""
  }

  private func setConnectionState(isConnecting: Bool, isConnected: Bool) {
    self.isConnecting = isConnecting
    self.isConnected = isConnected
  }

  private static func formatCoordinate(_ value: Double?) -> String {
    guard let value else { return "" }
    return String(format: "%.8f°", value)
  }

  private static func formatDistance(_ value: Double?) -> String {
    guard let value else { return "" }
    return String(format: "%.3f m", value)
  }
}
