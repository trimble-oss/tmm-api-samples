using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui_sample.BusinessLogic
{
  public class appRegister
  {
    //private string clientID { get; set; }
    //private string callback { get; set; }

    //private string status { get; set; }

    //private string message { get; set; }

    //private string registrationResult { get; set; }

    //private int apiPort { get; set; }

    //private int locationV2Port { get; set; }

    //public appRegister(string ClientID, string Callback, string Status, string RegistrationResult, int ApiPort, int LocationV2Port)
    //{
    //  clientID = ClientID;
    //  callback = Callback;
    //  status = Status;
    //  registrationResult = RegistrationResult;
    //  apiPort = ApiPort;
    //  locationV2Port = LocationV2Port;
    //}

    public static string Register(string callback, string status, string message, string registrationResult, int apiPort, int locationV2Port)
    {
      string url = $"{callback}?id = tmmRegister & status ={status}&message ={message}&registrationResult={registrationResult}&apiPort ={apiPort}&locationV2Port={locationV2Port}";
      return url;
    }
  }
}
