package com.example.kotlin_sample.tmmapiservices

import com.example.kotlin_sample.AppPreferences
import com.example.kotlin_sample.accesscode.AccessCodeV1
import com.example.kotlin_sample.accesscode.AccessCodeV2
import com.example.kotlin_sample.accesscode.AccessCodeVersion
import com.example.kotlin_sample.models.PortInfo
import com.example.kotlin_sample.models.ReceiverInfo
import java.time.Instant
import java.util.UUID
import java.util.concurrent.TimeUnit
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject

object RestApiService {
  private const val HOST = "tmm-api-local.fieldsystems.trimble.com"
  private val jsonMediaType = "application/json; charset=utf-8".toMediaType()

  private val json = Json {
    ignoreUnknownKeys = true
    isLenient = true
  }

  private val client: OkHttpClient = LocalTmmSsl.applyTo(
    OkHttpClient.Builder()
      .connectTimeout(15, TimeUnit.SECONDS)
      .readTimeout(60, TimeUnit.SECONDS)
      .writeTimeout(30, TimeUnit.SECONDS),
  ).build()

  /** Defaults to V2. Change to [AccessCodeVersion.V1] to use Basic auth instead. */
  var accessCodeVersion: AccessCodeVersion = AccessCodeVersion.V2

  private fun baseUrl(): String = "https://$HOST:${PortInfo.apiSecurePort}/"

  suspend fun getPublicKey(): String? = withContext(Dispatchers.IO) {
    val request = Request.Builder()
      .url(baseUrl() + "api/v1/publicKey")
      .get()
      .build()

    // Short timeout: used to probe whether TMM is reachable on the default port.
    val probeClient = client.newBuilder()
      .callTimeout(1, TimeUnit.SECONDS)
      .connectTimeout(1, TimeUnit.SECONDS)
      .readTimeout(1, TimeUnit.SECONDS)
      .build()

    runCatching {
      probeClient.newCall(request).execute().use { response ->
        if (!response.isSuccessful) {
          return@use null
        }
        response.body?.string()
      }
    }.getOrNull()
  }

  suspend fun getReceiver(appPreferences: AppPreferences): ReceiverInfo? =
    withContext(Dispatchers.IO) {
      val request = Request.Builder()
        .url(baseUrl() + "api/v1/receiver")
        .header("Authorization", buildAuthorizationHeader(appPreferences))
        .get()
        .build()

      runCatching {
        client.newCall(request).execute().use { response ->
          if (!response.isSuccessful) {
            return@use null
          }
          val body = response.body?.string() ?: return@use null
          json.decodeFromString<ReceiverInfo>(body)
        }
      }.getOrNull()
    }

  suspend fun putReceiver(appPreferences: AppPreferences, isConnected: Boolean) =
    withContext(Dispatchers.IO) {
      val payload = JSONObject().put("isConnected", isConnected).toString()
      val request = Request.Builder()
        .url(baseUrl() + "api/v1/receiver")
        .header("Authorization", buildAuthorizationHeader(appPreferences))
        .put(payload.toRequestBody(jsonMediaType))
        .build()

      runCatching {
        client.newCall(request).execute().use { response ->
          if (!response.isSuccessful) {
            android.util.Log.w("RestApiService", "putReceiver failed: ${response.code}")
          }
        }
      }
    }

  private fun buildAuthorizationHeader(appPreferences: AppPreferences): String {
    val appId = runCatching { UUID.fromString(appPreferences.appId.trim()) }
      .getOrElse {
        throw InvalidApplicationIdException("Invalid App ID \"${appPreferences.appId}\"")
      }

    val now = Instant.now()
    return when (accessCodeVersion) {
      AccessCodeVersion.V1 -> "Basic ${AccessCodeV1.generate(appId, now)}"
      AccessCodeVersion.V2 -> "AccessCodeV2 ${AccessCodeV2.generate(appId, now)}"
    }
  }
}
