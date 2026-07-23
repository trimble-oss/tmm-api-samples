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

    return new RegistrationDetails
    {
      RegistrationResult = queryDictionary["registrationResult"] ?? string.Empty,
      LocationPort = ParsePort(queryDictionary, "locationPort"),
      LocationSecurePort = ParsePort(queryDictionary, "locationSecurePort"),
      ApiPort = ParsePort(queryDictionary, "apiPort"),
      ApiSecurePort = ParsePort(queryDictionary, "apiSecurePort"),
      LocationV2Port = ParsePort(queryDictionary, "locationV2Port"),
      LocationV2SecurePort = ParsePort(queryDictionary, "locationV2SecurePort"),
    };
  }

  private static int ParsePort(NameValueCollection queryDictionary, string key)
  {
    int.TryParse(queryDictionary[key], out int port);
    return port;
  }

  public void HandleUri(System.Uri uri)
  {
    if (uri.AbsoluteUri.StartsWith(RegisterCallbackUri))
    {
      _registrationResult?.TrySetResult(uri);
    }
  }
}
