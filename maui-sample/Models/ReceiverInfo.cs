namespace MauiSample.Models
{
  public class ReceiverInfo
  {
    public bool IsReceiverConfigured { get; set; }
    public string? BluetoothName { get; set; }
    public string? BluetoothAddress { get; set; }
    public string? ReceiverBrand { get; set; }
    public string? ReceiverModel { get; set; }
    public string? ReceiverSerialNumber { get; set; }
    public bool IsConnected { get; set; }
    public bool IsSigninRequired { get; set; }
    public bool IsSignedIn { get; set; }
  }
}
