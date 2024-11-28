using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui_sample.Platforms.Windows
{
  public static class UrlSchemeService
  {
    public static async Task CallAsync(string uri,
                                     string details,
                                     Func<string, Task> returnCallback)
    {
      await Task.Run(() => { });
    }
  }
}
