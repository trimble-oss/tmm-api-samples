package com.example.kotlin_sample

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.google.android.material.textfield.TextInputEditText
import com.google.android.material.textfield.TextInputLayout
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.Date

class MainActivity : AppCompatActivity() {

  private lateinit var startForResult: ActivityResultLauncher<Intent>
//   Assign variable called startForResult (with property lateinit https://kotlinlang.org/docs/properties.html#late-initialized-properties-and-variables)
//   While creating an instance of ActivityResultLauncher

  private var registrationResult: String? = null

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
          val apiPort = data.getIntExtra("apiPort", -1)
          val positionsPort = data.getIntExtra("locationPort", -1)
          val positionsV2Port = data.getIntExtra("locationV2Port", -1)

          // Handle the retrieved data here
          println("Registration Result: $registrationResult")
          println("API Port: $apiPort")
          println("Positions Port: $positionsPort")
          println("Positions V2 Port: $positionsV2Port")
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
    val appIDInput: TextInputEditText = findViewById(R.id.appIDEditText)
    val buttonRegister: Button = findViewById(R.id.registerButton)
    buttonRegister.setOnClickListener {
//      Checks whether the input is the same as the ID stored in the property file
//      val appIDInput = appIDInput.text.toString()
//      if (androidRegistration(appIDInput))
//      {
//        println("Same")

//      }
//      else
//      {
//        println("Not same")
//      }
        println(sendCustomIntent("com.trimble.tmm.REGISTER"))
        if (registrationResult == "OK")
        {
          println("Registration success")
        }
        else
        {
          println("Please register your app.")
        }
    }

//    Get receiver button
    val appID = BuildConfig.appID
    val utcTime = Date()
    val accessCode = AccessCodeGenerator.generateAccessCode(appID, utcTime)
    val authorizationHeader = "Basic, $accessCode"

    val retrofit = Retrofit.Builder()
      .baseUrl("http://localhost:9637")
      .client(OkHttpClient.Builder().build())
      .addConverterFactory(GsonConverterFactory.create())
      .build()

    val apiService = retrofit.create(ApiService::class.java)

    CoroutineScope(Dispatchers.IO).launch {
      try {
        val response = apiService.getReceiverData(authorizationHeader)
        withContext(Dispatchers.Main) {
          // Handle the response data
          Toast.makeText(this@MainActivity, "Data: ${response.data}", Toast.LENGTH_SHORT).show()
        }
      } catch (e: Exception) {
        withContext(Dispatchers.Main) {
          // Handle the failure
          Toast.makeText(this@MainActivity, "Failure: ${e.message}", Toast.LENGTH_SHORT).show()
        }
      }
    }

    val getReceiverBut: Button = findViewById(R.id.getReceiverButton)
    getReceiverBut.setOnClickListener {
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
      }
      else
      {
        startStopBut.text = startText
      }
    }
  }

//  Method checks whether the value is the same as the stored value
//  private fun androidRegistration(input: String): Boolean {
//    val appID = BuildConfig.appID
//    return appID == input
//  }

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
