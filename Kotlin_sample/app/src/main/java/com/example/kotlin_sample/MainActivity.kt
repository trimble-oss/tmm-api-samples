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
import io.ktor.http.HttpStatusCode
import io.ktor.http.URLProtocol
import io.ktor.websocket.*
import org.json.JSONObject
import java.util.Date
import kotlinx.serialization.json.*

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
//    val test: Button = findViewById(R.id.testButton)
//    test.setOnClickListener {
//      println(sendCustomIntent("com.trimble.tmm.GNSS_STATUS", false))
//    }


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
      CoroutineScope(Dispatchers.IO).launch {
        try {
          if (startStopBut.text == startText) {
            if (registrationResult == "OK") {
              val response = checkReceiverConnection()
              var isConnected = response.bodyAsText()
              isConnected = JSONObject(isConnected).getString("isConnected")

              withContext(Dispatchers.Main) {
                if (isConnected == "true") {
                  startWebSocket()
                  startStopBut.text = stopText
                } else {
                  sendCustomIntent("com.trimble.tmm.RECEIVERSELECTION", false)
                }
              }
            } else {
              withContext(Dispatchers.Main) {
                Toast.makeText(this@MainActivity, getString(R.string.register_error_text), Toast.LENGTH_SHORT).show()
              }
            }
          } else {
            stopWebSocket()
            withContext(Dispatchers.Main) {
              startStopBut.text = startText
            }
          }
        } catch (e: Exception) {
          withContext(Dispatchers.Main) {
            println("Error: ${e.message}")
          }
        }
      }
    }

  }

  private fun getReceiverName() {
    CoroutineScope(Dispatchers.IO).launch {
      try {
        val response = checkReceiverConnection()
        val receiverTextBox: TextInputEditText = findViewById(R.id.receiverNameText)

        if (registrationResult == "OK") {
//          Only runs if app is registered.
//          Displays the receiver name to the text box
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

    return client.get("http://localhost:9637/api/v1/receiver") {
      header("Authorization", authorizationHeader)
    }
  }

  private fun startWebSocket() {
    CoroutineScope(Dispatchers.IO).launch {
      socket = websocketLocationV2.webSocketSession {
        url {
          protocol = URLProtocol.WS
          host = "localhost"
          port = positionsV2Port
        }
      }

      while (socket?.isActive == true) {
        val frame = socket?.incoming?.receive()
        if  (frame is Frame.Text) {
          val jsonString = frame.readText()
          withContext(Dispatchers.Main) {
            val latTextBox: TextInputEditText = findViewById(R.id.latitudeTextField)
            val longTextBox: TextInputEditText = findViewById(R.id.longitudeTextField)
            val altTextBox: TextInputEditText = findViewById(R.id.altitudeTextField)

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
      }
    }
  }

  private fun stopWebSocket() {
    CoroutineScope(Dispatchers.IO).launch {
      try {
        socket?.close(CloseReason(CloseReason.Codes.NORMAL, "Client closed connection"))
        websocketLocationV2.close()
      } catch (e: Exception) {
        withContext(Dispatchers.Main) {
        println(e.message)
        }
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

//  Occurs when app is closed
  override fun onDestroy() {
    super.onDestroy()
    client.close()
  }
}
