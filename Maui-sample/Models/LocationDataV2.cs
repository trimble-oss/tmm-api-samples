using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Maui_sample
{
  public class LocationDataV2
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

    [JsonProperty("totalSatInUse")]
    public int TotalSatInUse { get; set; }

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

    [JsonProperty("sourceReferenceFrameName")]
    public string SourceReferenceFrameName { get; set; }

    [JsonProperty("sourceReferenceEpoch")]
    public double SourceReferenceEpoch { get; set; }

    [JsonProperty("sourceReferenceFrameEpoch")]
    public double SourceReferenceFrameEpoch { get; set; }

    [JsonProperty("targetReferenceFrameEpoch")]
    public double TargetReferenceFrameEpoch { get; set; }

    [JsonProperty("targetReferenceFrameName")]
    public string TargetReferenceFrameName { get; set; }

    [JsonProperty("imuAlignmentStatus")]
    public int ImuAlignmentStatus { get; set; }

    [JsonProperty("isTIP")]
    public bool IsTIP { get; set; }

    [JsonProperty("pitch")]
    public double? Pitch { get; set; }

    [JsonProperty("roll")]
    public double? Roll { get; set; }

    [JsonProperty("yaw")]
    public double? Yaw { get; set; }

    [JsonProperty("pitchPrecision")]
    public double? PitchPrecision { get; set; }

    [JsonProperty("rollPrecision")]
    public double? RollPrecision { get; set; }

    [JsonProperty("yawPrecision")]
    public double? YawPrecision { get; set; }

    [JsonProperty("igsAntenna")]
    public string IGSAntenna { get; set; }

    [JsonProperty("antennaHeight")]
    public double AntennaHeight { get; set; }
  }

  public class LocationSatellite
  {
    [JsonProperty("Id")]
    public int Id { get; set; }

    [JsonProperty("Elv")]
    public int Elevation { get; set; }

    [JsonProperty("Azm")]
    public int Azimuth { get; set; }

    [JsonProperty("Snr")]
    public int Snr { get; set; }

    [JsonProperty("Use")]
    public bool InUse { get; set; }

    [JsonProperty("Type")]
    public int Type { get; set; }

    public override string ToString()
    {
      return GetType().GetProperties()
          .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
          .Aggregate(
              new StringBuilder(),
              (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
              sb => sb.ToString());
    }

    public string AsString()
    {
      string type = Type switch
      {
        0 => "GPS",
        1 => "SBAS",
        2 => "GLONASS",
        3 => "OMNISTAR",
        4 => "GALILEO",
        5 => "BEIDOU",
        6 => "QZSS",
        7 => "IRNSS",
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
      };

      return $"{type} {Id} Elv={Elevation} Azm={Azimuth} Snr={Snr} InUse={InUse}";
    }
  }
}
