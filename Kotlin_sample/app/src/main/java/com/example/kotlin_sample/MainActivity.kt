package com.example.kotlin_sample

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import android.widget.Toast
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.google.android.material.textfield.TextInputEditText
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import io.ktor.client.*
import io.ktor.client.engine.cio.CIO
import io.ktor.client.plugins.websocket.DefaultClientWebSocketSession
import io.ktor.client.plugins.websocket.WebSockets
import io.ktor.client.plugins.websocket.webSocketSession
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.statement.HttpResponse
import io.ktor.client.statement.bodyAsText
import io.ktor.http.URLProtocol
import io.ktor.websocket.CloseReason
import io.ktor.websocket.Frame
import io.ktor.websocket.close
import io.ktor.websocket.readText
import kotlinx.coroutines.isActive
import org.json.JSONObject
import java.util.Date

class MainActivity : AppCompatActivity() {

  private lateinit var startForResult: ActivityResultLauncher<Intent>
//   Assign variable called startForResult (with property lateinit https://kotlinlang.org/docs/properties.html#late-initialized-properties-and-variables)
//   While creating an instance of ActivityResultLauncher

//  Values returned from registration intent
  private var registrationResult: String? = null
  private var apiPort: Int = -1
  private var positionsPort: Int = -1
  private var positionsV2Port: Int = -1

//  Web socket
//  https://ktor.io/docs/client-engines.html#limitations
  private val websocketLocationV2 = HttpClient(CIO) {
    install(WebSockets)
}
  private var socket: DefaultClientWebSocketSession? = null

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    enableEdgeToEdge()
    setContentView(R.layout.activity_main)
    ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main)) { v, insets ->
      val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
      v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
      insets
    }

    startForResult = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
      // Result from the StartActivityForResult(). The data can be extracted with a lambda.
      if (result.resultCode == RESULT_OK) {
        val data: Intent? = result.data

        if (data != null) {
          registrationResult = data.getStringExtra("registrationResult")
          apiPort = data.getIntExtra("apiPort", -1)
          positionsPort = data.getIntExtra("locationPort", -1)
          positionsV2Port = data.getIntExtra("locationV2Port", -1)

          // Handle the retrieved data here
          println("Registration Result: $registrationResult")
          println("API Port: $apiPort")
          println("Positions Port: $positionsPort")
          println("Positions V2 Port: $positionsV2Port")
        }
        if (registrationResult == "OK")
        {
          println("Registration success")
        }
        else
        {
          println("Please register your app.")
        }
      }
    }

//      Version - Shows the version as text
    val versionText: TextView = findViewById(R.id.versionText)
    var versionNumber = applicationContext.packageManager.getPackageInfo(
      applicationContext.packageName,
      0
    ).versionName
    versionText.text = getString(R.string.version, versionNumber)

//        Input field for App ID
    val buttonRegister: Button = findViewById(R.id.registerButton)
    buttonRegister.setOnClickListener {
        println(sendCustomIntent("com.trimble.tmm.REGISTER"))
    }

//    Get receiver button
    val getReceiverBut: Button = findViewById(R.id.getReceiverButton)
    getReceiverBut.setOnClickListener {

      val appID = BuildConfig.appID
      val utcTime = Date()
      val accessCode = AccessCodeGenerator.generateAccessCode(appID, utcTime)
      val authorizationHeader = "Basic $accessCode"

      //HTTP client
      val client = HttpClient(CIO)
      CoroutineScope(Dispatchers.IO).launch {
        try {
          val response: HttpResponse = client.get("http://localhost:9637/api/v1/receiver") {
            header("Authorization", authorizationHeader)
          }

          val receiverTextBox: TextInputEditText = findViewById(R.id.receiverNameText)

          if (registrationResult == "OK") {
            var receiverName = response.bodyAsText()
//            payload = JSONObject(payload).toString(4)
            receiverName = JSONObject(receiverName).getString("bluetoothName")
            withContext(Dispatchers.Main) {
              Toast.makeText(this@MainActivity, receiverName, Toast.LENGTH_LONG).show()
              receiverTextBox.setText(receiverName)
            }
          } else {
            withContext(Dispatchers.Main) {
              Toast.makeText(this@MainActivity, "Error: ${response.status} or app not registered", Toast.LENGTH_SHORT).show()
              receiverTextBox.setText(getString(R.string.error_text))
            }
          }
        } catch (e:Exception) {
          withContext(Dispatchers.Main) {
            println("Error: ${e.message}")
          }
        } finally {
          client.close()
        }
      }
    }

    // Start/Stop position stream button button
    val startStopBut: Button = findViewById(R.id.startStopButton)

    val startText = getString(R.string.start)
    val stopText = getString(R.string.stop)
//    This is the preferred way to use hard-coded values
    startStopBut.setOnClickListener {
      if (startStopBut.text == startText)
      {
        startStopBut.text = stopText
        startWebSocket()
      }
      else
      {
        startStopBut.text = startText
        stopWebSocket()
      }
    }
  }

  private fun startWebSocket() {
    CoroutineScope(Dispatchers.IO).launch {
      try {
          socket = websocketLocationV2.webSocketSession {
            url {
              protocol = URLProtocol.WS
              host = "localhost"
              port = apiPort
            }
          }
        socket?.send(Frame.Text("Hello, WebSocket!"))
        while (socket?.isActive == true) {
          val frame = socket?.incoming?.receive() as? Frame.Text
          frame?.let {
            val receivedText = it.readText()
            println("Received: $receivedText")
          }
        }
      } catch (e: Exception) {
        println(e.message)
      }
    }
  }
  private fun stopWebSocket() {
    CoroutineScope(Dispatchers.IO).launch {
      try {
        socket?.close(CloseReason(CloseReason.Codes.NORMAL, "Client closed connection"))
        websocketLocationV2.close()
      } catch (e: Exception) {
        println(e.message)
      }
    }
  }

  private fun sendCustomIntent(action: String) {
//    Gets the app ID from the property file. Just one of few ways to not use hard-coded value
//    This was the simplest way, not necessarily the best
    val appID = BuildConfig.appID

    // Launching the intent and passing the AppID to the intent
    val intent = Intent(action)
    intent.putExtra("applicationID", appID)
    startForResult.launch(intent)
  }
}
