package com.example.kotlin_sample

import android.content.ActivityNotFoundException
import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.util.Log
import android.widget.Button
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import kotlin.math.log

class RestfulApiActivity : AppCompatActivity() {
  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    enableEdgeToEdge()
    setContentView(R.layout.activity_restful_api)
    ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main)) { v, insets ->
      val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
      v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
      insets
    }
    val getAntennaButton: Button = findViewById(R.id.getAntennaButton)
    getAntennaButton.setOnClickListener {
      val getAntennaURI = Uri.parse("com.trimble.tmm.OPENANTENNAHEIGHT")
      val getAntennaIntent = Intent(Intent.ACTION_VIEW, getAntennaURI)
      getAntennaIntent.setPackage("com.trimble.tmmtestapp")
      getAntennaIntent.resolveActivity(packageManager)?.let {
        startActivity(getAntennaIntent)
      }
    }
    }
  }
