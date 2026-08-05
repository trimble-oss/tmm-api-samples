package com.example.kotlin_sample.accesscode

import java.security.MessageDigest
import java.time.Instant
import java.time.ZoneOffset
import java.time.format.DateTimeFormatter
import java.util.Base64
import java.util.UUID

object AccessCodeV1 {
  private val iso8601Formatter: DateTimeFormatter =
    DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ssX")
      .withZone(ZoneOffset.UTC)

  fun generate(appId: UUID, utcTime: Instant): String {
    val lowercaseId = appId.toString().lowercase()
    val iso8601Time = iso8601Formatter.format(utcTime)
    val plaintextAccessCode = lowercaseId + iso8601Time
    val hashedBytes = MessageDigest.getInstance("SHA-256")
      .digest(plaintextAccessCode.toByteArray(Charsets.UTF_8))
    return Base64.getEncoder().encodeToString(hashedBytes)
  }
}
