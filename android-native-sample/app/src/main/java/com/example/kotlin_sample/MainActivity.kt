package com.example.kotlin_sample

import android.os.Bundle
import android.text.Editable
import android.text.TextWatcher
import androidx.activity.enableEdgeToEdge
import androidx.activity.viewModels
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.core.view.isVisible
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import com.example.kotlin_sample.databinding.ActivityMainBinding
import com.example.kotlin_sample.tmmapiservices.PlatformRequestService
import com.example.kotlin_sample.ui.MainUiEvent
import com.example.kotlin_sample.ui.MainViewModel
import kotlinx.coroutines.launch

class MainActivity : AppCompatActivity() {
  private lateinit var binding: ActivityMainBinding

  private val platformRequestService by lazy { PlatformRequestService(this) }
  private val appPreferences by lazy { AppPreferences(this) }

  private val viewModel: MainViewModel by viewModels {
    MainViewModel.Factory(appPreferences, platformRequestService)
  }

  private var suppressAppIdWatcher = false

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    enableEdgeToEdge()
    binding = ActivityMainBinding.inflate(layoutInflater)
    setContentView(binding.root)

    ViewCompat.setOnApplyWindowInsetsListener(binding.main) { view, insets ->
      val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
      view.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
      insets
    }

    binding.appIdEditText.setText(viewModel.uiState.value.applicationId)
    binding.appIdEditText.addTextChangedListener(object : TextWatcher {
      override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) = Unit
      override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) = Unit
      override fun afterTextChanged(s: Editable?) {
        if (!suppressAppIdWatcher) {
          viewModel.onApplicationIdChanged(s?.toString().orEmpty())
        }
      }
    })

    binding.connectButton.setOnClickListener { viewModel.connect() }
    binding.disconnectButton.setOnClickListener { viewModel.disconnect() }

    lifecycleScope.launch {
      repeatOnLifecycle(Lifecycle.State.STARTED) {
        launch {
          viewModel.uiState.collect { state ->
            if (binding.appIdEditText.text?.toString() != state.applicationId) {
              suppressAppIdWatcher = true
              binding.appIdEditText.setText(state.applicationId)
              binding.appIdEditText.setSelection(state.applicationId.length)
              suppressAppIdWatcher = false
            }

            binding.statusText.text = state.statusMessage
            binding.latitudeValue.text = state.latitude
            binding.longitudeValue.text = state.longitude
            binding.altitudeValue.text = state.altitude
            binding.accuracyValue.text = state.accuracy

            binding.connectButton.isEnabled = state.canConnect
            binding.disconnectButton.isEnabled = state.canDisconnect
            binding.progressBar.isVisible = state.isConnecting
          }
        }

        launch {
          viewModel.events.collect { event ->
            when (event) {
              is MainUiEvent.Alert -> {
                AlertDialog.Builder(this@MainActivity)
                  .setTitle(event.title)
                  .setMessage(event.message)
                  .setPositiveButton(android.R.string.ok, null)
                  .show()
              }

              is MainUiEvent.ConfirmReceiverNotConfigured -> {
                AlertDialog.Builder(this@MainActivity)
                  .setTitle(R.string.receiver_not_configured_title)
                  .setMessage(event.message)
                  .setPositiveButton(R.string.open_tmm) { _, _ ->
                    viewModel.openReceiverSelection()
                  }
                  .setNegativeButton(android.R.string.cancel, null)
                  .show()
              }
            }
          }
        }
      }
    }
  }
}
