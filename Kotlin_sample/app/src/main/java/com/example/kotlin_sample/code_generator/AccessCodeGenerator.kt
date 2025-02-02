package com.example.kotlin_sample.code_generator

import java.security.MessageDigest
import java.text.SimpleDateFormat
import java.util.Base64
import java.util.Locale
import java.util.TimeZone
import java.util.Date


fun generateAccessCode(appID: String, utcTime: Date): String {
//  Generates the Access Code from the app id and the current time.
//   Used when trying to access the receiver API or any API that requires it.
  val lowercaseID = appID.lowercase(Locale.getDefault())

  val iso8601Format = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.US).apply {
    timeZone = TimeZone.getTimeZone("UTC")
  }
  val iso8601Time = iso8601Format.format(utcTime)

  val plaintextAccessCode = lowercaseID + iso8601Time
  val utf8Bytes = plaintextAccessCode.toByteArray(Charsets.UTF_8)
  val hashedBytes = MessageDigest.getInstance("SHA-256").digest(utf8Bytes)
  val base64String = Base64.getEncoder().encodeToString(hashedBytes)

  return base64String
}
