using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;
using Maui_sample.Models;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Maui_sample.WinUI
{
  /// <summary>
  /// Provides application-specific behavior to supplement the default Application class.
  /// </summary>
  public partial class App : MauiWinUIApplication
  {
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
      InitializeComponent();

      // Windows will launch a new instance of TMM with every URI activation. We only want the
      // 'main' instance to handle the URI activation.
      var mainInstance = AppInstance.FindOrRegisterForKey("sampleapp");
      if (mainInstance.IsCurrent)
      {
        // This is the 'main' instance handle the URI
        AppInstance.GetCurrent().Activated += OnActivated;

        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
        HandleProtocolActivation(args);
      }
      else
      {
        // This is not the 'main' instance. Redirect the URI to the 'main'
        // instance, end kill this instance.
        mainInstance.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs()).Wait();
        Process.GetCurrentProcess().Kill();
      }
    }

    private void OnActivated(object sender, AppActivationArguments args)
    {
      HandleProtocolActivation(args);
    }

    private void HandleProtocolActivation(AppActivationArguments args)
    {
      if (args.Kind == ExtendedActivationKind.Protocol && args.Data is ProtocolActivatedEventArgs protocolArgs)
      {
        Uri uri = protocolArgs.Uri;
        if (uri.AbsolutePath.StartsWith("sampleapp://response/tmmRegister"))
        {
          // this is the callbackUri sent to TMM earlier
          NameValueCollection queryDictionary = HttpUtility.ParseQueryString(uri.Query);
          string id = queryDictionary["id"]; // "tmmRegister"
          string status = queryDictionary["status"]; // “success” or “error”
          string message = queryDictionary["message "]; // Additional information
          string registrationResult = queryDictionary["registrationResult"]; // “OK”, “NoNetwork”, or “Unauthorized”
          int.TryParse(queryDictionary["apiPort"], out var apiPort); // The REST API is at $"WS://localhost:{apiPort}"

          PortInfo.APIPort = apiPort;
        }
      }
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
  }

}
