namespace MauiSample.Models;

public sealed class LocationV2DataMessage
{
  public double? Latitude { get; init; }

  public double? Longitude { get; init; }

  public double? Altitude { get; init; }

  public double? Speed { get; init; }

  public double? Bearing { get; init; }

  public string? SolutionType { get; init; }

  public double? Hrms { get; init; }

  public double? Vrms { get; init; }
}
