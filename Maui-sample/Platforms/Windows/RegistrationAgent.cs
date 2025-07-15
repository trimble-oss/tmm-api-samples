using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using ABI.System;
using Maui_sample.Models;

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
      string callback = "tmmapisample://response";
      string uriString = $"trimbleMobileManager://request/tmmRegister?callback={callback}&applicationId={applicationID}";

      Debug.WriteLine($"Launching URI for Windows registration: {uriString}");

      _registrationResult = new();

      bool success = await Launcher.Default.TryOpenAsync(uriString);
      if (!success)
      {
        Debug.WriteLine("Failed to launch the registration URI. Is the target application installed?");
      }

      System.Uri responseUri = await _registrationResult.Task;

      return GetRegistrationDetails(responseUri);
    }
    catch (System.Exception ex)
    {
      Debug.WriteLine($"Error launching registration URI: {ex.Message}");
    }

    return null;
  }

  private RegistrationDetails GetRegistrationDetails(System.Uri uri)
  {
    NameValueCollection queryDictionary = HttpUtility.ParseQueryString(uri.Query);

    string result = queryDictionary["registrationResult"] ?? string.Empty;
    string portString = queryDictionary["apiPort"] ?? string.Empty;
    int.TryParse(portString, out int portNumber);
    return new RegistrationDetails
    {
      RegistrationResult = result,
      ApiPort = portNumber
    };
  }

  public void HandleUri(System.Uri uri)
  {
    _registrationResult?.TrySetResult(uri);
  }
}
