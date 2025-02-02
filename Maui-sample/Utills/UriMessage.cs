using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
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
