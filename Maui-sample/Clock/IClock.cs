using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui_sample.Clock
{
  public interface IClock
  {
    DateTime Now { get; }
    DateTime UtcNow { get; }
  }
}
