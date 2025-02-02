using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Maui_sample
{
  public class UriMessage : ValueChangedMessage<Uri>
  {
    public UriMessage(Uri uri) : base (uri)
    {
    }
  }
}
