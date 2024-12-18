package com.example.kotlin_sample

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.google.android.material.textfield.TextInputEditText
import com.google.android.material.textfield.TextInputLayout

class MainActivity : AppCompatActivity() {
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
    val tileText: TextInputLayout = findViewById(R.id.appIDTextTitle)
    val appIDInput: TextInputEditText = findViewById(R.id.appIDEditText)

    // Start/Stop button
    val startStopBut: Button = findViewById(R.id.startStopButton)

    startStopBut.setOnClickListener {
      if (startStopBut.text == "Start")
      {
        startStopBut.text = "Stop"
        androidRegistration()
      }
      else
      {
        startStopBut.text = "Start"
      }
    }
  }

  private fun androidRegistration() {
    val appID = BuildConfig.appID
    println(appID)
    val registrationIntent = Intent("com.trimble.tmm.REGISTER")
    registrationIntent.putExtra("applicationID", appID)
//    startActivityForResult(intent, registerRequestCode)
  }
}
