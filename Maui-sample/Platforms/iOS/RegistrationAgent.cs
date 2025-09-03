using Foundation;
using Maui_sample.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Maui_sample;

public partial class RegistrationAgent
{
  private TaskCompletionSource<System.Uri>? _registrationResult;

  private RegistrationAgent()
  {
  }

  public partial async Task<RegistrationDetails?> RegisterAsync(string applicationID)
  {
    try
    {
      // TMM will open this URL to pass back the registration result.
      // Ensure this matches the scheme registered in Info.plist under CFBundleURLTypes.
      string returnURL = "tmmapisample://response";

      var payload = new JObject
      {
        ["application_id"] = applicationID,
        ["returl"] = returnURL
      };

      string jsonPayload = payload.ToString(Newtonsoft.Json.Formatting.None);

      byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);
      string base64Payload = Convert.ToBase64String(jsonBytes);

      string uriString = $"tmmregister://?{base64Payload}";

      Debug.WriteLine($"Launching corrected URI for iOS registration: {uriString}");

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
        registrationDetails.RegistrationResult = data["registrationResult"].ToString();
      }
      if (data.ContainsKey("apiPort"))
      {
        registrationDetails.ApiPort = int.Parse(data["apiPort"].ToString());
      }
    }

    return registrationDetails;
  }

  public void HandleUri(Uri uri)
  {
    _registrationResult?.TrySetResult(uri);
  }
}
