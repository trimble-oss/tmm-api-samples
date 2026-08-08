import Foundation

@MainActor
final class AppPreferences {
  static let shared = AppPreferences()

  private let defaults = UserDefaults.standard
  private let appIdKey = "SampleAppID"

  var appId: String {
    get { defaults.string(forKey: appIdKey) ?? "" }
    set { defaults.set(newValue, forKey: appIdKey) }
  }
}
