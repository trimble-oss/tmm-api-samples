//
//  AccessCodeGenerator.swift
//  swift_sample
//
//  Created by kea-build on 15/01/2025.
//

import Foundation
import CommonCrypto

struct AccessCodeGenerator {
    static func generateAccessCode(appID: String, utcTime: Date) -> String? {
        let lowercaseID = appID.lowercased()
        
        // Format utcTime as an ISO8601 compliant string
        let iso8601TimeFormatter = ISO8601DateFormatter()
        iso8601TimeFormatter.timeZone = TimeZone(secondsFromGMT: 0)
        let iso8601Time = iso8601TimeFormatter.string(from: utcTime)
        
        let plaintextAccessCode = lowercaseID + iso8601Time
        guard let utf8Data = plaintextAccessCode.data(using: .utf8) else {
            return nil
        }
        
        var hashedBytes = [UInt8](repeating: 0, count: Int(CC_SHA256_DIGEST_LENGTH))
        utf8Data.withUnsafeBytes {
            _ = CC_SHA256($0.baseAddress, CC_LONG(utf8Data.count), &hashedBytes)
        }
        
        let hashedData = Data(hashedBytes)
        let base64String = hashedData.base64EncodedString()
        return base64String
    }
}
