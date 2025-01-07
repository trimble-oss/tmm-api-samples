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
import kotlinx.coroutines.*
import io.ktor.client.*
import io.ktor.client.engine.cio.CIO
import io.ktor.client.plugins.websocket.*
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.statement.HttpResponse
import io.ktor.client.statement.bodyAsText
import io.ktor.http.HttpMethod
import io.ktor.http.HttpStatusCode
import io.ktor.http.URLProtocol
import io.ktor.websocket.*
import org.json.JSONObject
import java.util.Date

class MainActivity : AppCompatActivity() {

  private lateinit var startForResult: ActivityResultLauncher<Intent>
//   Assign variable called startForResult (with property lateinit https://kotlinlang.org/docs/properties.html#late-initialized-properties-and-variables)
//   While creating an instance of ActivityResultLauncher

//  AppID
private val appID = BuildConfig.appID

//  Values returned from registration intent
  private var registrationResult: String? = null
  private var apiPort: Int = -1
  private var positionsV2Port: Int = -1

//  Http client - Can't be reused for web socket as it causes `Parent process has finished` exception
  private val client = HttpClient(CIO)

//  Web socket client
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
          positionsV2Port = data.getIntExtra("locationV2Port", -1)

          // Debugging purposes
          println("Registration Result: $registrationResult")
          println("API Port: $apiPort")
          println("Positions V2 Port: $positionsV2Port")
        }
      }
    }

//      Version - Shows the version as text
    val versionText: TextView = findViewById(R.id.versionText)
    val versionNumber = applicationContext.packageManager.getPackageInfo(
      applicationContext.packageName,
      0
    ).versionName
    versionText.text = getString(R.string.version, versionNumber)

//        register button
    val buttonRegister: Button = findViewById(R.id.registerButton)
    buttonRegister.setOnClickListener {
        println(sendCustomIntent("com.trimble.tmm.REGISTER", true))
    }

    // Test for sending intent without AppID
    val test: Button = findViewById(R.id.testButton)
    test.setOnClickListener {
      println(sendCustomIntent("com.trimble.tmm.GNSS_STATUS", false))
    }


//    Get receiver button
    val getReceiverBut: Button = findViewById(R.id.getReceiverButton)
    getReceiverBut.setOnClickListener {
      getReceiverName()
    }

    // Start/Stop position stream button button
    val startStopBut: Button = findViewById(R.id.startStopButton)

    val startText = getString(R.string.start)
    val stopText = getString(R.string.stop)
//    This is the preferred way to use hard-coded values
    startStopBut.setOnClickListener {
      if (startStopBut.text == startText)
//      TODO: Add check for app registration
      {
//        Change text from start --> stop and run the web socket
        startStopBut.text = stopText
        startWebSocket()
      }
      else
      {
//      Change text from stop --> start and stop the web socket
        startStopBut.text = startText
        stopWebSocket()
      }
    }
  }

  private fun getReceiverName() {
    CoroutineScope(Dispatchers.IO).launch {
      try {
        val response = checkReceiverConnection()
        val receiverTextBox: TextInputEditText = findViewById(R.id.receiverNameText)

        if (registrationResult == "OK") {
          var receiverName = response.bodyAsText()
          receiverName = JSONObject(receiverName).getString("bluetoothName")
          withContext(Dispatchers.Main) {
            receiverTextBox.setText(receiverName)
          }
        } else {
          withContext(Dispatchers.Main) {
//            Asks user to register app, otherwise will show a Http status code
            if (response.status != HttpStatusCode.OK) {
              Toast.makeText(this@MainActivity, getString(R.string.error_text, response.status), Toast.LENGTH_SHORT).show()
            } else {
              Toast.makeText(this@MainActivity, getString(R.string.register_error_text), Toast.LENGTH_SHORT).show()
            }
          }
        }
      } catch (e: Exception) {
        withContext(Dispatchers.Main) {
          println("Error: ${e.message}")
        }
      }
    }
  }

  private suspend fun checkReceiverConnection(): HttpResponse {
    val utcTime = Date()
    val accessCode = AccessCodeGenerator.generateAccessCode(appID, utcTime)
    val authorizationHeader = "Basic $accessCode"
    // Get authorization header with access code through the generator

        return client.get("http://localhost:$apiPort/api/v1/receiver") {
          header("Authorization", authorizationHeader)
        }
  }

  override fun onDestroy() {
    super.onDestroy()
    // Close the client when the activity is destroyed
    client.close()
  }

  private fun startWebSocket() {
    CoroutineScope(Dispatchers.IO).launch {
      val response = checkReceiverConnection()
      var isConnected = response.bodyAsText()
      isConnected = JSONObject(isConnected).getString("isConnected")

      if (isConnected == "true")
      {
        println("Connected")
        socket = websocketLocationV2.webSocketSession {
          url {
            protocol = URLProtocol.WS
            host = "localhost"
            port = positionsV2Port
          }
        }
      } else {
        println("Not connected")
      }
      socket?.send(Frame.Text("Hello, WebSocket!"))
      while (socket?.isActive == true) {
        val frame = socket?.incoming?.receive()
        if  (frame is Frame.Binary) {
          val data = frame.readBytes()
          val jsonString = data.decodeToString()
          withContext(Dispatchers.Main) {
            val latTextBox: TextInputEditText = findViewById(R.id.latitudeTextField)
            val longTextBox: TextInputEditText = findViewById(R.id.longitudeTextField)
            val altTextBox: TextInputEditText = findViewById(R.id.altitudeTextField)

            try {
//              val latJsonObject = Json.parseToJsonElement
            } catch (e: Exception) {
              println(e.message)
            }
          }
        }
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

  private fun sendCustomIntent(action: String, includeAppID: Boolean) {


    // Launching the intent and passing the AppID to the intent
    // Most intents will either need or not need AppID, which the function can handle
    val intent = Intent(action)

    if (includeAppID) {
      // Gets the app ID from the property file. Just one of few ways to not use hard-coded value
      // This was the simplest way, not necessarily the best
      intent.putExtra("applicationID", appID)
    }
    startForResult.launch(intent)
    }
  }
