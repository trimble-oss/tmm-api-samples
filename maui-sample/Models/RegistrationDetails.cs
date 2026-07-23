namespace MauiSample.Models
{
  public class RegistrationDetails
  {
    public string RegistrationResult { get; set; } = string.Empty;
    public int LocationPort { get; set; }
    public int LocationSecurePort { get; set; }
    public int ApiPort { get; set; }
    public int ApiSecurePort { get; set; }
    public int LocationV2Port { get; set; }
    public int LocationV2SecurePort { get; set; }
  }
}
