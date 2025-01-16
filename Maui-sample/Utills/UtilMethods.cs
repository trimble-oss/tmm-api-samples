using System.Diagnostics;

namespace Maui_sample.Utills
{
  public class UtilMethods
  {
    public static async Task<bool> checkRequest(string requestUri)
    {
      if (await Launcher.Default.CanOpenAsync(requestUri))
      {
        bool result = await Launcher.Default.OpenAsync(requestUri);
        Debug.WriteLine($"Result: {result}");
        return result;
      }
      return false;
    }
  }
}
