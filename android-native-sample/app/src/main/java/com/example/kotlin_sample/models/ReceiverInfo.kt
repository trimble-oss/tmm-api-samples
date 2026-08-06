package com.example.kotlin_sample.models

import kotlinx.serialization.Serializable

@Serializable
data class ReceiverInfo(
  val isReceiverConfigured: Boolean = false,
  val bluetoothName: String? = null,
  val bluetoothAddress: String? = null,
  val receiverBrand: String? = null,
  val receiverModel: String? = null,
  val receiverSerialNumber: String? = null,
  val isConnected: Boolean = false,
  val isSigninRequired: Boolean = false,
  val isSignedIn: Boolean = false,
)
