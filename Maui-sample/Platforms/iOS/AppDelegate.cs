using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Foundation;
using Maui_sample.Models;
using UIKit; 

namespace Maui_sample;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
  protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

  public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
  {
    if (url?.AbsoluteString is not null)
    {
      var uri = new System.Uri(url.AbsoluteString);
      WeakReferenceMessenger.Default.Send(new UriMessage(uri));
      return true;
    }

    Debug.WriteLine("Received a null or invalid URL.");
    return false;
  }
}
