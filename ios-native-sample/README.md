# iOS Native Sample

Swift sample demonstrating basic Trimble Mobile Manager (TMM) API workflows:

1. Register with TMM (starts TMM / discovers API ports when needed)
2. Fetch the RSA public key and authenticate with **Access Code V2** (default)
3. Ensure a GNSS receiver is configured and connected
4. Stream **Location V2** positions over a secure WebSocket

## Prerequisites

- Xcode 16+ (iOS 17 deployment target)
- Trimble Mobile Manager installed on the device or simulator
- A TMM Application ID (GUID)
- Host resolution for `tmm-api-local.fieldsystems.trimble.com` → `127.0.0.1` (TMM provides local HTTPS/WSS)

## Run

Open `ios-native-sample/` in Xcode and run the `ios-native-sample` scheme.

Set your own **Development Team** under Signing & Capabilities if you are not on the Trimble team.

## Usage

1. Enter your Application ID
2. Tap **Connect**
3. If no receiver is configured, choose **Open TMM** and select a receiver
4. Watch latitude / longitude / altitude / accuracy update from the position stream
5. Tap **Disconnect** to stop streaming

## Access Code versions

The sample defaults to Access Code **V2** (`Authorization: AccessCodeV2 …`).

To use V1 (`Authorization: Basic …`) instead, set:

```swift
RestApiService.accessCodeVersion = .v1
```

in `TmmApiServices/RestApiService.swift` (or before connecting).

## Project layout

```
ios-native-sample/
  AccessCode/          Access Code V1 / V2 generation
  Models/              DTOs and PortInfo
  TmmApiServices/      REST, WebSocket, TMM deep-link bridge
  MainViewModel.swift  Connect / disconnect orchestration
  ContentView.swift    Single-screen UI
```

## Related docs

https://developer.trimble.com/docs/mobile-manager
