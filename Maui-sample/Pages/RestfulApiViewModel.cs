using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Maui_sample
{
    class RestfulApiViewModel : ReactiveObject
    {
      [Reactive]
      public bool AreButtonsEnabled { get; set; } = true;
  }
}
