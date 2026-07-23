using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using MauiSample.Models;

namespace MauiSample;

public partial class PlatformRequestService
{
  private const string RegisterCallbackUri = "tmmapimauisample://response/tmmRegister";

  private TaskCompletionSource<Uri>? _registrationResult;

  partial void InitializePlatform()
  {
  }

  public partial async Task<RegistrationDetails?> RegisterAsync(string applicationID)
  {
    try
    {
      string callback = RegisterCallbackUri;
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
    if (uri.AbsoluteUri.StartsWith(RegisterCallbackUri))
    {
      _registrationResult?.TrySetResult(uri);
    }
  }
}
