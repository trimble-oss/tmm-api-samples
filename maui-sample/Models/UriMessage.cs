using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MauiSample.Models;

public class UriMessage : ValueChangedMessage<System.Uri>
{
  public UriMessage(System.Uri value) : base(value)
  {
  }
}
