package com.example.kotlin_sample.accesscode

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotEquals
import org.junit.Test
import java.time.Instant
import java.util.UUID

class AccessCodeV1Test {
  @Test
  fun generate_isDeterministicForSameInputs() {
    val appId = UUID.fromString("12345678-1234-1234-1234-123456789abc")
    val utcTime = Instant.parse("2024-02-22T18:00:00Z")

    val first = AccessCodeV1.generate(appId, utcTime)
    val second = AccessCodeV1.generate(appId, utcTime)

    assertEquals(first, second)
    assertEquals(44, first.length) // SHA-256 digest as Base64
  }

  @Test
  fun generate_changesWithTime() {
    val appId = UUID.fromString("12345678-1234-1234-1234-123456789abc")
    val first = AccessCodeV1.generate(appId, Instant.parse("2024-02-22T18:00:00Z"))
    val second = AccessCodeV1.generate(appId, Instant.parse("2024-02-22T18:00:01Z"))

    assertNotEquals(first, second)
  }
}
