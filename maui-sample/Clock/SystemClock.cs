using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSample.Clock
{
  public class SystemClock : IClock
  {
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
  }
}
