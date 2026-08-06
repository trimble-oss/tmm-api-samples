package com.example.kotlin_sample.models

data class RegistrationDetails(
  val registrationResult: String = "",
  val locationPort: Int = 0,
  val locationSecurePort: Int = 0,
  val apiPort: Int = 0,
  val apiSecurePort: Int = 0,
  val locationV2Port: Int = 0,
  val locationV2SecurePort: Int = 0,
)
