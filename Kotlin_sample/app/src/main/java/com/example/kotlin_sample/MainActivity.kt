package com.example.kotlin_sample

import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.CheckBox
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

//        CheckBox - Makes the checkbox hide/show the input field
    val checkB: CheckBox = findViewById(R.id.customIDTextCheckBox)
    val tileText: TextInputLayout = findViewById(R.id.appIDTextTitle)
    val appIDInput: TextInputEditText = findViewById(R.id.appIDEditText)
    checkB.setOnClickListener {
      if (checkB.isChecked) {
//          Set visibility property. To get visibility status remove the equality
        tileText.visibility = View.VISIBLE
        appIDInput.visibility = View.VISIBLE
      } else {
        tileText.visibility = View.GONE
        appIDInput.visibility = View.GONE
      }
    }
  }
}
