# Trimble Mobile Manager API Samples
A collection of sample applications that demonstrate basic use of Trimble Mobile Manager API (TMM API).

This repository contains the following sample applications:
- Cross platform Android, iOS, and Windows (.NET MAUI)
- Native Android (Kotlin) — [`android-native-sample/`](android-native-sample/)
- Native iOS (Swift) — [`ios-native-sample/`](ios-native-sample/)
- Web (React + Express)

Full TMM API documentation is available at https://developer.trimble.com/docs/mobile-manager

## Web Sample

The web sample demonstrates how to use TMM API in a web application. It lives in [`web-sample/`](web-sample/). From the repository root:

```bash
pnpm install
pnpm dev
```

See [`web-sample/README.md`](web-sample/README.md) for details.

## iOS Native Sample

The iOS native sample demonstrates the same Connect / Disconnect workflow as the MAUI and Android samples. It lives in [`ios-native-sample/`](ios-native-sample/).

Open `ios-native-sample/ios-native-sample.xcodeproj` in Xcode and run the `ios-native-sample` scheme.

See [`ios-native-sample/README.md`](ios-native-sample/README.md) for details.
