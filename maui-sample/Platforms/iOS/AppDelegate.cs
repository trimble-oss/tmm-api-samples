using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Foundation;
using MauiSample.Models;
using UIKit; 

namespace MauiSample;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
  protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

  public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
  {
    if (url?.AbsoluteString is not null)
    {
      var uri = new Uri(url.AbsoluteString);
      RegistrationAgent.Instance.HandleUri(uri);
      return true;
    }

    Debug.WriteLine("Received a null or invalid URL.");
    return false;
  }
}
