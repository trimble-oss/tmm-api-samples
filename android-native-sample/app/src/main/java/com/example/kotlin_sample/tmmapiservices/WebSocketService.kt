package com.example.kotlin_sample.tmmapiservices

import android.util.Log
import com.example.kotlin_sample.models.LocationV2DataMessage
import com.example.kotlin_sample.models.PortInfo
import java.util.concurrent.TimeUnit
import java.util.concurrent.atomic.AtomicReference
import kotlinx.coroutines.channels.awaitClose
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.callbackFlow
import kotlinx.serialization.json.Json
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.Response
import okhttp3.WebSocket
import okhttp3.WebSocketListener

class WebSocketService {
  private val json = Json {
    ignoreUnknownKeys = true
    isLenient = true
  }

  private val client: OkHttpClient = LocalTmmSsl.applyTo(
    OkHttpClient.Builder()
      .connectTimeout(15, TimeUnit.SECONDS)
      .readTimeout(0, TimeUnit.MILLISECONDS)
      .pingInterval(30, TimeUnit.SECONDS),
  ).build()

  private val activeSocket = AtomicReference<WebSocket?>(null)

  fun readPositions(): Flow<LocationV2DataMessage> = callbackFlow {
    val uri =
      "wss://tmm-api-local.fieldsystems.trimble.com:${PortInfo.locationV2SecurePort}/locationV2"
    val request = Request.Builder().url(uri).build()

    val listener = object : WebSocketListener() {
      override fun onMessage(webSocket: WebSocket, text: String) {
        runCatching {
          json.decodeFromString<LocationV2DataMessage>(text)
        }.onSuccess { position ->
          trySend(position)
        }.onFailure { error ->
          Log.w(TAG, "Failed to parse position message", error)
        }
      }

      override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
        Log.w(TAG, "WebSocket failure: ${t.message}")
        close(t)
      }

      override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
        webSocket.close(NORMAL_CLOSURE, null)
        close()
      }

      override fun onClosed(webSocket: WebSocket, code: Int, reason: String) {
        close()
      }
    }

    val socket = client.newWebSocket(request, listener)
    activeSocket.set(socket)

    awaitClose {
      activeSocket.compareAndSet(socket, null)
      socket.close(NORMAL_CLOSURE, "Cancelled")
    }
  }

  fun stop() {
    activeSocket.getAndSet(null)?.close(NORMAL_CLOSURE, "Stopped")
  }

  companion object {
    private const val TAG = "WebSocketService"
    private const val NORMAL_CLOSURE = 1000
  }
}
