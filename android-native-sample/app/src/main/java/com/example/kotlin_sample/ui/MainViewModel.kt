package com.example.kotlin_sample.ui

import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.example.kotlin_sample.AppPreferences
import com.example.kotlin_sample.accesscode.AccessCodeV2
import com.example.kotlin_sample.models.PortInfo
import com.example.kotlin_sample.tmmapiservices.InvalidApplicationIdException
import com.example.kotlin_sample.tmmapiservices.PlatformRequestService
import com.example.kotlin_sample.tmmapiservices.RestApiService
import com.example.kotlin_sample.tmmapiservices.WebSocketService
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class MainUiState(
  val applicationId: String = "",
  val statusMessage: String = "",
  val latitude: String = "",
  val longitude: String = "",
  val altitude: String = "",
  val accuracy: String = "",
  val isConnecting: Boolean = false,
  val isConnected: Boolean = false,
) {
  val canConnect: Boolean get() = !isConnecting && !isConnected
  val canDisconnect: Boolean get() = !isConnecting && isConnected
}

sealed interface MainUiEvent {
  data class Alert(val title: String, val message: String) : MainUiEvent
  data class ConfirmReceiverNotConfigured(val message: String) : MainUiEvent
}

class MainViewModel(
  private val appPreferences: AppPreferences,
  private val platformRequestService: PlatformRequestService,
  private val webSocketService: WebSocketService = WebSocketService(),
) : ViewModel() {

  private val _uiState = MutableStateFlow(
    MainUiState(applicationId = appPreferences.appId),
  )
  val uiState: StateFlow<MainUiState> = _uiState.asStateFlow()

  private val _events = MutableSharedFlow<MainUiEvent>(extraBufferCapacity = 1)
  val events: SharedFlow<MainUiEvent> = _events.asSharedFlow()

  private var positionJob: Job? = null

  fun onApplicationIdChanged(value: String) {
    appPreferences.appId = value
    _uiState.update { it.copy(applicationId = value) }
  }

  fun connect() {
    if (!_uiState.value.canConnect) {
      return
    }

    viewModelScope.launch {
      if (appPreferences.appId.isBlank()) {
        _uiState.update { it.copy(statusMessage = "Please enter an Application ID.") }
        _events.emit(MainUiEvent.Alert("Error", "Please enter an Application ID"))
        return@launch
      }

      setConnectionState(isConnecting = true, isConnected = false)
      _uiState.update { it.copy(statusMessage = "Connecting...") }

      try {
        _uiState.update { it.copy(statusMessage = "Getting public key...") }
        var publicKey = tryGetPublicKey()
        if (publicKey == null) {
          _uiState.update { it.copy(statusMessage = "Registering...") }
          if (!tryRegister()) {
            _uiState.update { it.copy(statusMessage = "Registration failed.") }
            return@launch
          }

          _uiState.update { it.copy(statusMessage = "Getting public key again...") }
          publicKey = tryGetPublicKey()
          if (publicKey == null) {
            _uiState.update { it.copy(statusMessage = "Failed to get public key.") }
            return@launch
          }
        }

        _uiState.update { it.copy(statusMessage = "Getting receiver info...") }
        var receiver = RestApiService.getReceiver(appPreferences)
        if (receiver == null) {
          _uiState.update { it.copy(statusMessage = "Failed to get receiver info.") }
          return@launch
        }

        if (!receiver.isReceiverConfigured) {
          _uiState.update {
            it.copy(statusMessage = "Receiver not configured. Select a receiver in TMM.")
          }
          _events.emit(
            MainUiEvent.ConfirmReceiverNotConfigured(
              "Connect to a receiver in TMM to start streaming positions.",
            ),
          )
          return@launch
        }

        if (!receiver.isConnected) {
          _uiState.update { it.copy(statusMessage = "Connecting to GNSS receiver...") }
          RestApiService.putReceiver(appPreferences, isConnected = true)
          receiver = RestApiService.getReceiver(appPreferences)
          if (receiver == null || !receiver.isConnected) {
            _uiState.update { it.copy(statusMessage = "Failed to connect to GNSS receiver.") }
            return@launch
          }
        }

        _uiState.update { it.copy(statusMessage = "Starting position stream...") }
        startPositionStream()
        _uiState.update { it.copy(statusMessage = "Connected.") }
        setConnectionState(isConnecting = false, isConnected = true)
      } catch (_: InvalidApplicationIdException) {
        _uiState.update { it.copy(statusMessage = "Invalid Application ID.") }
        _events.emit(MainUiEvent.Alert("Error", "The provided Application ID is invalid."))
        setConnectionState(isConnecting = false, isConnected = false)
      } catch (ex: Exception) {
        Log.e(TAG, "Connect failed", ex)
        _uiState.update { it.copy(statusMessage = "Connection failed.") }
        _events.emit(
          MainUiEvent.Alert("Error", "An unexpected error occurred while connecting."),
        )
        setConnectionState(isConnecting = false, isConnected = false)
      } finally {
        if (_uiState.value.isConnecting) {
          setConnectionState(isConnecting = false, isConnected = false)
        }
      }
    }
  }

  fun disconnect() {
    if (!_uiState.value.canDisconnect && !_uiState.value.isConnected) {
      return
    }

    setConnectionState(isConnecting = true, isConnected = false)
    _uiState.update { it.copy(statusMessage = "Disconnecting...") }

    positionJob?.cancel()
    positionJob = null
    webSocketService.stop()
    clearLocationData()

    _uiState.update { it.copy(statusMessage = "Disconnected.") }
    setConnectionState(isConnecting = false, isConnected = false)
  }

  fun openReceiverSelection() {
    platformRequestService.showReceiverSelection()
  }

  private suspend fun tryGetPublicKey(): String? {
    return try {
      val publicKey = RestApiService.getPublicKey()
      if (publicKey != null) {
        AccessCodeV2.setPublicKey(publicKey)
      }
      publicKey
    } catch (_: Exception) {
      null
    }
  }

  private suspend fun tryRegister(): Boolean {
    return try {
      val details = platformRequestService.registerAsync(appPreferences.appId)
      if (details == null || details.registrationResult.isBlank()) {
        return false
      }

      val result = details.registrationResult
      if (result.equals("OK", ignoreCase = true)) {
        PortInfo.locationPort = details.locationPort
        PortInfo.locationSecurePort = details.locationSecurePort
        PortInfo.apiPort = details.apiPort
        PortInfo.apiSecurePort = details.apiSecurePort
        PortInfo.locationV2Port = details.locationV2Port
        PortInfo.locationV2SecurePort = details.locationV2SecurePort
      }

      result.equals("OK", ignoreCase = true) || result.equals("success", ignoreCase = true)
    } catch (ex: Exception) {
      Log.e(TAG, "Registration failed", ex)
      false
    }
  }

  private fun startPositionStream() {
    positionJob?.cancel()
    positionJob = viewModelScope.launch {
      webSocketService.readPositions().collect { position ->
        _uiState.update {
          it.copy(
            latitude = formatCoordinate(position.latitude),
            longitude = formatCoordinate(position.longitude),
            altitude = formatDistance(position.altitude),
            accuracy = formatDistance(position.hrms),
          )
        }
      }
    }
  }

  private fun clearLocationData() {
    _uiState.update {
      it.copy(
        latitude = "",
        longitude = "",
        altitude = "",
        accuracy = "",
      )
    }
  }

  private fun setConnectionState(isConnecting: Boolean, isConnected: Boolean) {
    _uiState.update {
      it.copy(isConnecting = isConnecting, isConnected = isConnected)
    }
  }

  override fun onCleared() {
    positionJob?.cancel()
    webSocketService.stop()
    super.onCleared()
  }

  companion object {
    private const val TAG = "MainViewModel"

    private fun formatCoordinate(value: Double?): String =
      value?.let { String.format("%.8f°", it) }.orEmpty()

    private fun formatDistance(value: Double?): String =
      value?.let { String.format("%.3f m", it) }.orEmpty()
  }

  class Factory(
    private val appPreferences: AppPreferences,
    private val platformRequestService: PlatformRequestService,
  ) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
      if (modelClass.isAssignableFrom(MainViewModel::class.java)) {
        return MainViewModel(appPreferences, platformRequestService) as T
      }
      throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}")
    }
  }
}
