# Android Native Sample

Kotlin sample demonstrating basic Trimble Mobile Manager (TMM) API workflows:

1. Register with TMM (starts TMM / discovers API ports when needed)
2. Fetch the RSA public key and authenticate with **Access Code V2** (default)
3. Ensure a GNSS receiver is configured and connected
4. Stream **Location V2** positions over a secure WebSocket

## Prerequisites

- Android Studio (or JDK 17+ with Android SDK)
- Trimble Mobile Manager installed on the device/emulator
- A TMM Application ID (GUID)
- Host resolution for `tmm-api-local.fieldsystems.trimble.com` → `127.0.0.1` (TMM provides local HTTPS/WSS)

## Run

Open `android-native-sample/` in Android Studio and run the `app` configuration, or:

```bash
./gradlew :app:assembleDebug
```

## Usage

1. Enter your Application ID
2. Tap **Connect**
3. If no receiver is configured, choose **Open TMM** and select a receiver
4. Watch latitude / longitude / altitude / accuracy update from the position stream
5. Tap **Disconnect** to stop streaming

## Access Code versions

The sample defaults to Access Code **V2** (`Authorization: AccessCodeV2 …`).

To use V1 (`Authorization: Basic …`) instead, set:

```kotlin
RestApiService.accessCodeVersion = AccessCodeVersion.V1
```

in `tmmapiservices/RestApiService.kt` (or before connecting).

## Project layout

```
app/src/main/java/com/example/kotlin_sample/
  accesscode/          Access Code V1 / V2 generation
  models/              DTOs and PortInfo
  tmmapiservices/      REST, WebSocket, TMM intent bridge
  ui/                  ViewModel + UI state
  MainActivity.kt
```

## Related docs

https://developer.trimble.com/docs/mobile-manager
