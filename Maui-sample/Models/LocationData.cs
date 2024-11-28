using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Maui_sample
{
  public class LocationData
  {
    [JsonProperty("accuracy")]
    public float Accuracy { get; set; }

    [JsonProperty("altitude")]
    public double Altitude { get; set; }

    [JsonProperty("appVersion")]
    public string AppVersion { get; set; }

    [JsonProperty("battery")]
    public int? Battery { get; set; }

    [JsonProperty("bearing")]
    public double Bearing { get; set; }

    [JsonProperty("diffAge")]
    public double DiffAge { get; set; }

    [JsonProperty("diffID")]
    public string DiffId { get; set; }

    [JsonProperty("diffStatus")]
    public int DiffStatus { get; set; }

    [JsonProperty("geoidModel")]
    public string GeoidModel { get; set; }

    [JsonProperty("gpsTimeStamp")]
    public string GpsTimeStamp { get; set; }

    [JsonProperty("hdop")]
    public double Hdop { get; set; }

    [JsonProperty("hrms")]
    public double Hrms { get; set; }

    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("mockProvider")]
    public string MockProvider { get; set; }

    [JsonProperty("mslHeight")]
    public float? MslHeight { get; set; }

    [JsonProperty("pdop")]
    public double Pdop { get; set; }

    [JsonProperty("receiverModel")]
    public string ReceiverModel { get; set; }

    [JsonProperty("satelliteView")]
    public List<LocationSatellite> Satellites { get; set; }

    [JsonProperty("satellites")]
    public int SatelliteCount { get; set; }

    [JsonProperty("speed")]
    public double Speed { get; set; }

    [JsonProperty("subscriptionType")]
    public int SubscriptionType { get; set; }

    [JsonProperty("totalSatInView")]
    public int TotalSatellitesInView { get; set; }

    [JsonProperty("undulation")]
    public float? Undulation { get; set; }

    [JsonProperty("utcTime")]
    public float UtcTime { get; set; }

    [JsonProperty("utcTimeStamp")]
    public string UtcTimeStamp { get; set; }

    [JsonProperty("vdop")]
    public double Vdop { get; set; }

    [JsonProperty("verticalAccuracyMeters")]
    public float VerticalAccuracyMeters { get; set; }

    [JsonProperty("vrms")]
    public double Vrms { get; set; }
  }
}
