package com.example.kotlin_sample.utils

import com.example.kotlin_sample.MainActivity
import com.example.kotlin_sample.R
import android.content.Context
import com.google.android.material.textfield.TextInputEditText
import io.ktor.client.HttpClient
import io.ktor.client.engine.cio.CIO
import io.ktor.client.plugins.websocket.*
import io.ktor.http.URLProtocol
import io.ktor.websocket.*
import kotlinx.coroutines.*
import kotlinx.serialization.json.*

//  WebSocket client
//  https://ktor.io/docs/client-engines.html#limitations
class WebSocketManager {
  private val websocketLocationV2 = HttpClient(CIO) {
    install(WebSockets)
  }


  private var socket: DefaultClientWebSocketSession? = null

  fun startWebSocket(context: Context, positionsV2Port: Int) {
//    Ran when position stream starts and receiver is connected.
    CoroutineScope(Dispatchers.IO).launch {
      socket = websocketLocationV2.webSocketSession {
        url {
          protocol = URLProtocol.WS
          host = "localhost"
          port = positionsV2Port
        }
      }

//      Continuously receives the message until the start/stop position stream button is pressed again.
      while (socket?.isActive == true) {
        try {
          val frame = socket?.incoming?.receive()
          if (frame is Frame.Text) {
            val jsonString = frame.readText()
            withContext(Dispatchers.Main) {
              val latTextBox: TextInputEditText = (context as MainActivity).findViewById(R.id.latitudeTextField)
              val longTextBox: TextInputEditText = context.findViewById(R.id.longitudeTextField)
              val altTextBox: TextInputEditText = context.findViewById(R.id.altitudeTextField)

              try {
                val jsonObject = Json.parseToJsonElement(jsonString).jsonObject
                val latitude = jsonObject["latitude"]?.jsonPrimitive?.content
                val longitude = jsonObject["longitude"]?.jsonPrimitive?.content
                val altitude = jsonObject["altitude"]?.jsonPrimitive?.content

                latTextBox.setText(latitude)
                longTextBox.setText(longitude)
                altTextBox.setText(altitude)
              } catch (e: Exception) {
                println(e.message)
              }
            }
          }
        } catch (e: Exception) {
//          Exception here was causing app to crash because coroutine chanel was closed,
//          while the loop was still running.
          println(e.message)
        }
      }
    }
  }

  fun stopWebSocket(context: Context) {
//    Stops position stream when button is pressed after streaming has started.
    CoroutineScope(Dispatchers.IO).launch {
      try {
        socket?.close(CloseReason(CloseReason.Codes.NORMAL, "Stop button pressed"))
        withContext(Dispatchers.Main) {
          val latTextBox: TextInputEditText = (context as MainActivity).findViewById(R.id.latitudeTextField)
          val longTextBox: TextInputEditText = context.findViewById(R.id.longitudeTextField)
          val altTextBox: TextInputEditText = context.findViewById(R.id.altitudeTextField)

          latTextBox.setText("")
          longTextBox.setText("")
          altTextBox.setText("")
        }
      } catch (e: Exception) {
        withContext(Dispatchers.Main) {
          println(e.message)
        }
      }
    }
  }

  fun sessionClose() {
    websocketLocationV2.close()
  }
}
