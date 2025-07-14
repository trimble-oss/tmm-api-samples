using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Maui_sample.Models;

public class UriMessage : ValueChangedMessage<System.Uri>
{
  public UriMessage(System.Uri value) : base(value)
  {
  }
}
