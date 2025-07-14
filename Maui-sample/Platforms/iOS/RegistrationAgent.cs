using Foundation;
using Maui_sample.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Maui_sample;

internal partial class RegistrationAgent
{
  public partial async Task<RegistrationDetails?> RegisterAsync(string applicationID)
  {
    try
    {
      string callbackUrl = "tmmapisample://response";

      var payload = new JObject
      {
        ["application_id"] = applicationID,
        ["returl"] = callbackUrl
      };

      string jsonPayload = payload.ToString(Newtonsoft.Json.Formatting.None);

      byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);
      string base64Payload = Convert.ToBase64String(jsonBytes);

      string uriString = $"tmmregister://?{base64Payload}";

      Debug.WriteLine($"Launching corrected URI for iOS registration: {uriString}");

      bool success = await Launcher.Default.TryOpenAsync(uriString);

      if (!success)
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
}
