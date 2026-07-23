using Foundation;
using MauiSample.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace MauiSample;

public partial class PlatformRequestService
{
  private const string RegisterReturnUrl = "tmmapimauisample://response/register";

  private TaskCompletionSource<Uri>? _registrationResult;

  partial void InitializePlatform()
  {
  }

  public partial async Task<RegistrationDetails?> RegisterAsync(string applicationID)
  {
    try
    {
      // TMM will open this URL to pass back the registration result.
      // Ensure this matches the scheme registered in Info.plist under CFBundleURLTypes.
      var payload = new JObject
      {
        ["application_id"] = applicationID,
        ["returl"] = RegisterReturnUrl
      };

      string jsonPayload = payload.ToString(Newtonsoft.Json.Formatting.None);

      byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);
      string base64Payload = Convert.ToBase64String(jsonBytes);

      string uriString = $"tmmregister://?{base64Payload}";

      Debug.WriteLine($"Launching URI for iOS registration: {uriString}");

      _registrationResult = new();

      bool success = await Launcher.Default.TryOpenAsync(uriString);
      if (success)
      {
        Uri uri = await _registrationResult.Task;
        return GetRegistrationDetails(uri);
      }
      else
      {
        Debug.WriteLine("Failed to launch the registration URI. Is Trimble Mobile Manager installed and is 'tmmregister' in LSApplicationQueriesSchemes in Info.plist?");

        if (Application.Current?.MainPage != null)
        {
          await Application.Current.MainPage.DisplayAlert("Error", "Could not open Trimble Mobile Manager. Please ensure it is installed.", "OK");
        }
        else
        {
          Debug.WriteLine("Application.Current.MainPage is null. Cannot display alert.");
        }
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Error launching registration URI on iOS: {ex.Message}");

      if (Application.Current?.MainPage != null)
      {
        await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred during registration.", "OK");
      }
      else
      {
        Debug.WriteLine("Application.Current.MainPage is null. Cannot display alert.");
      }
    }

    return null;
  }

  private RegistrationDetails GetRegistrationDetails(System.Uri uri)
  {
    RegistrationDetails registrationDetails = new();

    string json = Encoding.UTF8.GetString(Convert.FromBase64String(uri.Query.Substring(1)));
    JObject data = JObject.Parse(json);
    if (data is not null)
    {
      if (data.ContainsKey("registrationResult"))
      {
        registrationDetails.RegistrationResult = data["registrationResult"]?.ToString() ?? string.Empty;
      }
      registrationDetails.LocationPort = ParsePort(data, "locationPort");
      registrationDetails.LocationSecurePort = ParsePort(data, "locationSecurePort");
      registrationDetails.ApiPort = ParsePort(data, "apiPort");
      registrationDetails.ApiSecurePort = ParsePort(data, "apiSecurePort");
      registrationDetails.LocationV2Port = ParsePort(data, "locationV2Port");
      registrationDetails.LocationV2SecurePort = ParsePort(data, "locationV2SecurePort");
    }

    return registrationDetails;
  }

  private static int ParsePort(JObject data, string key)
  {
    if (data.ContainsKey(key) && int.TryParse(data[key]?.ToString(), out int port))
    {
      return port;
    }
    return 0;
  }

  public void HandleUri(Uri uri)
  {
    if (uri.AbsoluteUri.StartsWith(RegisterReturnUrl))
    {
      _registrationResult?.TrySetResult(uri);
    }
  }
}
