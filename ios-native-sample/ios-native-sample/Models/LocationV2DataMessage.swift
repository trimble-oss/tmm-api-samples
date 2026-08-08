import Foundation

struct LocationV2DataMessage: Codable {
  var latitude: Double?
  var longitude: Double?
  var altitude: Double?
  var speed: Double?
  var bearing: Double?
  var solutionType: String?
  var hrms: Double?
  var vrms: Double?
}
