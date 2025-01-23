package com.example.kotlin_sample.utils

import android.content.Context
import android.widget.Toast
import com.example.kotlin_sample.MainActivity
import com.example.kotlin_sample.R
import com.example.kotlin_sample.code_generator.generateAccessCode
import com.google.android.material.textfield.TextInputEditText
import io.ktor.client.HttpClient
import io.ktor.client.engine.cio.CIO
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.statement.HttpResponse
import io.ktor.client.statement.bodyAsText
import io.ktor.http.HttpStatusCode
import kotlinx.coroutines.*
import org.json.JSONObject
import java.util.Date

class ApiUtils {
  private val client = HttpClient(CIO)

  fun getReceiverName(context: Context, registrationResult: String, appIDInput: TextInputEditText) {
    CoroutineScope(Dispatchers.IO).launch {
      try {
        val response = checkReceiverConnection(appIDInput)
        val receiverTextBox: TextInputEditText = (context as MainActivity).findViewById(R.id.receiverNameText)

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
              Toast.makeText(context, context.getString(R.string.error_text, response.status), Toast.LENGTH_SHORT).show()
            } else {
              Toast.makeText(context, context.getString(R.string.register_error_text), Toast.LENGTH_SHORT).show()
            }
          }
        }
      } catch (e: Exception) {
        withContext(Dispatchers.Main) {
//          If TMM isn't open it will throw an exception
          println("Error: ${e.message}")
          Toast.makeText(context, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
        }
      }
    }
  }

  suspend fun checkReceiverConnection(appIDInput: TextInputEditText): HttpResponse {
    val utcTime = Date()
    val accessCode = generateAccessCode(appIDInput.text.toString(), utcTime)
    val authorizationHeader = "Basic $accessCode"
    // Get authorization header with access code through the generator

    return client.get("http://localhost:9637/api/v1/receiver") {
      header("Authorization", authorizationHeader)
    }
  }

  fun clientClose() {
    client.close()
  }
}
