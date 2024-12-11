using System.Diagnostics;
using Windows.System;
// Windows.System is required for LaunchQuerySupportType and LaunchQuerySupportStatus

using Launcher = Windows.System.Launcher;
// This line is required for the Launcher class


namespace Maui_sample.Platforms.Windows
{
  internal class WinRequests
  {
    private static Func<string, Task>? s_returnCallback;
    public static async Task RegisterWithTMMAsync(string request, string callback, Func<string, Task> returnCallback)
    {
      try
      {
        var requestToLaunch = new Uri(request);
        bool supportsRequest = await Launcher.QueryUriSupportAsync(requestToLaunch, LaunchQuerySupportType.Uri) == LaunchQuerySupportStatus.Available;

        if (supportsRequest)
        {
          bool result = await Launcher.LaunchUriAsync(requestToLaunch);
          Debug.WriteLine($"result: {result}");

          if (returnCallback != null)
          {
            s_returnCallback = returnCallback;
          }
        }
        else
        {
          Debug.WriteLine("Request not supported.");
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error in Method: " + ex.Message);
      }
    }
  }
}
