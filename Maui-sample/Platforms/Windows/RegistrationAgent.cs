using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using ABI.System;
using Maui_sample.Models;

namespace Maui_sample;

internal partial class RegistrationAgent
{
  public RegistrationAgent()
  {
  }

  public partial async Task<RegistrationDetails?> RegisterAsync(string applicationID)
  {
    try
    {
      string callback = "tmmapisample://response";
      string uriString = $"trimbleMobileManager://request/tmmRegister?callback={callback}&applicationId={applicationID}";

      Debug.WriteLine($"Launching URI for Windows registration: {uriString}");

      bool success = await Launcher.Default.TryOpenAsync(uriString);

      if (!success)
      {
        Debug.WriteLine("Failed to launch the registration URI. Is the target application installed?");
      }
    }
    catch (System.Exception ex)
    {
      Debug.WriteLine($"Error launching registration URI: {ex.Message}");
    }

    return null;
  }
}
