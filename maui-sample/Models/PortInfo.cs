using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSample.Models
{
  internal class PortInfo
  {
    public static int LocationPort { get; set; } = 9635;
    public static int LocationSecurePort { get; set; } = 9636;
    public static int ApiPort { get; set; } = 9637;
    public static int ApiSecurePort { get; set; } = 9638;
    public static int LocationV2Port { get; set; } = 9639;
    public static int LocationV2SecurePort { get; set; } = 9640;
  }
}
