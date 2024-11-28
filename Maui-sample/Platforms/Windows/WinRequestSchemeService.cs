using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Launcher = Windows.System.Launcher;
using Maui_sample;
using Newtonsoft.Json.Linq;

namespace Maui_sample.Platforms.Windows
{
  //private static Func<string, Task> _returnCallback;
  //private static WinRequestSchemePage _winRequestSchemePage;
  internal class WinRequestSchemeService
  {
    //public static async Task CallAsync(string request, string callback, Func<string, Task> returnCallback)
    //{
    //  try
    //  {
    //    var requestToLaunch = new Uri(request);
    //    bool supportsRequest = await Launcher.QueryUriSupportAsync(requestToLaunch, LaunchQuerySupportType.Uri) == LaunchQuerySupportStatus.Available;

    //    if (supportsRequest)
    //    {
    //      bool result = await Launcher.LaunchUriAsync(requestToLaunch);
    //      Console.WriteLine(result);

    //      if (returnCallback != null)
    //      {
    //        _returnCallback = returnCallback;
    //      }
    //    }
    //    else
    //    {
    //      Console.WriteLine("Request not supported.");
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    Console.WriteLine("Error in CallAsync: " + ex.Message);
    //  }
    //}

    //public static async Task HandleResponse(string responseJson)
    //{
    //  if (_winRequestSchemePage != null)
    //  {
    //    await _winRequestSchemePage.DefaultResponseDisplayAsync(responseJson);
    //  }
    //  else
    //  {
    //    Console.WriteLine("No WinRequestSchemePage instance available to handle the response.");
    //  }
    //}

    //public static async void HandleUri(Uri uri)
    //{
    //  var queryParameters = uri.Query.TrimStart('?')
    //      .Split('&', StringSplitOptions.RemoveEmptyEntries)
    //      .Select(param => param.Split('='))
    //      .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));

    //  var jsonObject = new JObject();
    //  foreach (var param in queryParameters)
    //  {
    //    jsonObject[param.Key] = param.Value;
    //  }

    //  var responseJson = jsonObject.ToString();

    //  if (_returnCallback != null)
    //  {
    //    await _returnCallback(responseJson);
    //  }
    //  else
    //  {
    //    await HandleResponse(responseJson);
    //  }
    //}
  }
}
