using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MauiSample
{
  public class UriMessage : ValueChangedMessage<Uri>
  {
    public UriMessage(Uri uri) : base (uri)
    {
    }
  }
}
