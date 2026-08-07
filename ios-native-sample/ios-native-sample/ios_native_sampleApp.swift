import SwiftUI

@main
struct ios_native_sampleApp: App {
  @State private var viewModel = MainViewModel()

  var body: some Scene {
    WindowGroup {
      ContentView(viewModel: viewModel)
        .onOpenURL { url in
          PlatformRequestService.shared.handleIncomingURL(url)
        }
    }
  }
}
