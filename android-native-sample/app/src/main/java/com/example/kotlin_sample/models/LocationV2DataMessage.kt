package com.example.kotlin_sample.models

import kotlinx.serialization.Serializable

@Serializable
data class LocationV2DataMessage(
  val latitude: Double? = null,
  val longitude: Double? = null,
  val altitude: Double? = null,
  val speed: Double? = null,
  val bearing: Double? = null,
  val solutionType: String? = null,
  val hrms: Double? = null,
  val vrms: Double? = null,
)
