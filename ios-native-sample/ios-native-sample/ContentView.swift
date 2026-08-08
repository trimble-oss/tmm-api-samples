import SwiftUI

struct ContentView: View {
  @Bindable var viewModel: MainViewModel

  var body: some View {
    ScrollView {
      VStack(spacing: 20) {
        VStack(alignment: .leading, spacing: 8) {
          Text("Application ID")
            .foregroundStyle(.secondary)
          TextField("Enter your Application ID", text: $viewModel.applicationId)
            .textFieldStyle(.roundedBorder)
            .autocorrectionDisabled()
            .textInputAutocapitalization(.never)
        }

        HStack(spacing: 12) {
          Button("Connect") {
            Task { await viewModel.connect() }
          }
          .buttonStyle(.borderedProminent)
          .disabled(!viewModel.canConnect)

          Button("Disconnect") {
            viewModel.disconnect()
          }
          .buttonStyle(.borderedProminent)
          .disabled(!viewModel.canDisconnect)
        }

        VStack(alignment: .leading, spacing: 8) {
          Text("Status")
            .foregroundStyle(.secondary)
          Text(viewModel.statusMessage)
            .frame(maxWidth: .infinity, alignment: .leading)
        }

        locationCard
      }
      .frame(maxWidth: 320)
      .padding()
    }
    .frame(maxWidth: .infinity)
    .alert(
      alertTitle,
      isPresented: alertBinding,
      actions: {
        Button("OK", role: .cancel) {
          viewModel.clearPendingEvent()
        }
      },
      message: {
        Text(alertMessage)
      }
    )
    .confirmationDialog(
      "Receiver Not Configured",
      isPresented: confirmBinding,
      titleVisibility: .visible,
      actions: {
        Button("Open TMM") {
          viewModel.openReceiverSelection()
          viewModel.clearPendingEvent()
        }
        Button("Cancel", role: .cancel) {
          viewModel.clearPendingEvent()
        }
      },
      message: {
        Text(confirmMessage)
      }
    )
  }

  private var locationCard: some View {
    VStack(spacing: 12) {
      locationRow(label: "Latitude", value: viewModel.latitudeText)
      locationRow(label: "Longitude", value: viewModel.longitudeText)
      locationRow(label: "Altitude", value: viewModel.altitudeText)
      locationRow(label: "Accuracy", value: viewModel.accuracyText)
    }
    .padding(20)
    .frame(maxWidth: .infinity, alignment: .leading)
    .background(Color(white: 0.35))
    .clipShape(RoundedRectangle(cornerRadius: 8))
  }

  private func locationRow(label: String, value: String) -> some View {
    HStack {
      Text(label)
        .foregroundStyle(Color(white: 0.75))
        .fontWeight(.bold)
      Spacer()
      Text(value)
        .foregroundStyle(.white)
        .fontWeight(.bold)
    }
  }

  private var alertBinding: Binding<Bool> {
    Binding(
      get: {
        if case .alert = viewModel.pendingEvent { return true }
        return false
      },
      set: { if !$0 { viewModel.clearPendingEvent() } }
    )
  }

  private var confirmBinding: Binding<Bool> {
    Binding(
      get: {
        if case .confirmReceiverNotConfigured = viewModel.pendingEvent { return true }
        return false
      },
      set: { if !$0 { viewModel.clearPendingEvent() } }
    )
  }

  private var alertTitle: String {
    if case .alert(let title, _) = viewModel.pendingEvent { return title }
    return ""
  }

  private var alertMessage: String {
    if case .alert(_, let message) = viewModel.pendingEvent { return message }
    return ""
  }

  private var confirmMessage: String {
    if case .confirmReceiverNotConfigured(let message) = viewModel.pendingEvent { return message }
    return ""
  }
}

#Preview {
  ContentView(viewModel: MainViewModel())
}
