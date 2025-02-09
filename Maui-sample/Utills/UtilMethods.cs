using System;

namespace Maui_sample.Utills
{
  public class UtilMethods
  {
    public static async Task<bool> checkRequest(string requestId, string callback, string appID = "")
    {
      string requestUri;
      if (string.IsNullOrEmpty(appID))
      {
        requestUri = $"trimbleMobileManager://request/{requestId}?callback={callback}";
      }
      else
      {
        requestUri = $"trimbleMobileManager://request/{requestId}?applicationId={appID}&callback={callback}";
      }
      // Runs whenever a URL wants to be opened.
      if (await Launcher.Default.CanOpenAsync(requestUri))
      {
        bool result = await Launcher.Default.OpenAsync(requestUri);
        return result;
      }
      return false;
    }
  }
}
