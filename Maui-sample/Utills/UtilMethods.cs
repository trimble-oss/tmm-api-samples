using System.Diagnostics;

namespace Maui_sample.Utills
{
  public class UtilMethods
  {
    public static async Task<bool> checkRequest(string requestUri)
    {
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
