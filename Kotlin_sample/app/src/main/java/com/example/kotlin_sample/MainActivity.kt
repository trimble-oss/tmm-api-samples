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

class MainActivity : AppCompatActivity() {
  private lateinit var CustomActivityResultContract: ActivityResultLauncher<Uri>

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    enableEdgeToEdge()
    setContentView(R.layout.activity_main)
    ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main)) { v, insets ->
      val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
      v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
      insets
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
      val appIDInput = appIDInput.text.toString()
      if (androidRegistration(appIDInput))
      {
        println("Same")
        CustomActivityResultContract = registerForActivityResult(CustomActivityResultContract()) { uri? -> uri.let {
          handleCallbackUri(it)
        }
         }
      }
      else
      {
        println("Not same")
      }
    }


    // Start/Stop button
    val startStopBut: Button = findViewById(R.id.startStopButton)

    startStopBut.setOnClickListener {
      if (startStopBut.text == "Start position stream")
      {
        startStopBut.text = "Stop position stream"
      }
      else
      {
        startStopBut.text = "Start position stream"
      }
    }
  }

  private fun androidRegistration(input: String): Boolean {
    val appID = BuildConfig.appID
    return appID == input
  }

  private fun handleCallbackUri (uri: Uri) {
    val callbackParams = uri.queryParameterNames
    for (param in callbackParams) {
      val value = uri.getQueryParameter(param)
      println("Callback param: $param, value: $value")
    }
  }
}
