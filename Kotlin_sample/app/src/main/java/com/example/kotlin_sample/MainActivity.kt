package com.example.kotlin_sample

import android.content.Intent
import android.os.Bundle
import android.widget.*
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.google.android.material.textfield.TextInputEditText
import kotlinx.coroutines.*
import io.ktor.client.statement.bodyAsText
import org.json.JSONObject


import com.example.kotlin_sample.utils.ApiUtils
import com.example.kotlin_sample.utils.WebSocketManager

class MainActivity : AppCompatActivity() {

  private lateinit var startForResult: ActivityResultLauncher<Intent>
//   Assign variable called startForResult (with property lateinit https://kotlinlang.org/docs/properties.html#late-initialized-properties-and-variables)
//   While creating an instance of ActivityResultLauncher

//  AppID
  private lateinit var appIDInput: TextInputEditText

//  Values returned from registration intent
  private var registrationResult: String? = null
  private var apiPort: Int = -1
  private var positionsV2Port: Int = -1

//  Http client - Can't be reused for web socket as it causes `Parent process has finished` exception
  private val client = ApiUtils()

//  Web socket
  private val webSocket = WebSocketManager()

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

          if (registrationResult != "OK") {
            Toast.makeText(this@MainActivity, "Registration Error: $registrationResult", Toast.LENGTH_SHORT).show()
          } else {
            Toast.makeText(this@MainActivity, "Registration successful", Toast.LENGTH_SHORT).show()
          }
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
    appIDInput = findViewById(R.id.registerEditField)
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
      client.getReceiverName(this@MainActivity, registrationResult ?: "", appIDInput)
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
              val response = client.checkReceiverConnection(appIDInput)
              var isConnected = response.bodyAsText()
              isConnected = JSONObject(isConnected).getString("isConnected")

              withContext(Dispatchers.Main) {
                if (isConnected == "true") {
                  webSocket.startWebSocket(this@MainActivity, positionsV2Port)
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
            webSocket.stopWebSocket(this@MainActivity)
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

  private fun sendCustomIntent(action: String, includeAppID: Boolean) {


    // Launching the intent and passing the AppID to the intent
    // Most intents will either need or not need AppID, which the function can handle
    val intent = Intent(action)

    if (includeAppID) {
      // Gets the app ID from the property file. Just one of few ways to not use hard-coded value
      // This was the simplest way, not necessarily the best
      intent.putExtra("applicationID", appIDInput.text.toString())
    }
    startForResult.launch(intent)
    }

//  Occurs when app is closed
  override fun onDestroy() {
    super.onDestroy()
    client.clientClose()
    webSocket.sessionClose()
  }
}
