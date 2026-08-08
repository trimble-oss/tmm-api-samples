import Foundation

struct ReceiverInfo: Codable {
  var isReceiverConfigured: Bool
  var bluetoothName: String?
  var bluetoothAddress: String?
  var receiverBrand: String?
  var receiverModel: String?
  var receiverSerialNumber: String?
  var isConnected: Bool
  var isSigninRequired: Bool
  var isSignedIn: Bool
}
