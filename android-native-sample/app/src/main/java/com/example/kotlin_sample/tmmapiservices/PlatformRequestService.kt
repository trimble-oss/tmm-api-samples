package com.example.kotlin_sample.tmmapiservices

import android.app.Activity
import android.content.Intent
import androidx.activity.ComponentActivity
import androidx.activity.result.ActivityResult
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import com.example.kotlin_sample.models.RegistrationDetails
import kotlin.coroutines.resume
import kotlinx.coroutines.suspendCancellableCoroutine

class PlatformRequestService(private val activity: ComponentActivity) {
  private var pendingContinuation: ((ActivityResult) -> Unit)? = null

  private val activityResultLauncher: ActivityResultLauncher<Intent> =
    activity.registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
      pendingContinuation?.invoke(result)
      pendingContinuation = null
    }

  suspend fun registerAsync(applicationId: String): RegistrationDetails? {
    return try {
      val intent = Intent("com.trimble.tmm.REGISTER").apply {
        putExtra("applicationID", applicationId)
      }
      val result = launchForResult(intent) ?: return null
      if (result.resultCode != Activity.RESULT_OK) {
        return null
      }
      parseRegistrationDetails(result.data)
    } catch (_: Exception) {
      null
    }
  }

  fun showReceiverSelection() {
    try {
      activity.startActivity(Intent("com.trimble.tmm.RECEIVERSELECTION"))
    } catch (_: Exception) {
      // TMM may not be installed.
    }
  }

  private suspend fun launchForResult(intent: Intent): ActivityResult? =
    suspendCancellableCoroutine { continuation ->
      pendingContinuation = { result ->
        if (continuation.isActive) {
          continuation.resume(result)
        }
      }
      continuation.invokeOnCancellation {
        pendingContinuation = null
      }
      activityResultLauncher.launch(intent)
    }

  private fun parseRegistrationDetails(data: Intent?): RegistrationDetails? {
    if (data == null) {
      return null
    }
    return RegistrationDetails(
      registrationResult = data.getStringExtra("registrationResult").orEmpty(),
      locationPort = data.getIntExtra("locationPort", 0),
      locationSecurePort = data.getIntExtra("locationSecurePort", 0),
      apiPort = data.getIntExtra("apiPort", 0),
      apiSecurePort = data.getIntExtra("apiSecurePort", 0),
      locationV2Port = data.getIntExtra("locationV2Port", 0),
      locationV2SecurePort = data.getIntExtra("locationV2SecurePort", 0),
    )
  }
}
