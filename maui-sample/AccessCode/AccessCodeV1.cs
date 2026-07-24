using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MauiSample.AccessCode;

public static class AccessCodeV1
{
  public static string Generate(Guid appID, DateTime utcTime)
  {
    string lowercaseID = appID.ToString("D");
    // Format utcTime as an ISO8601 compliant string, like this: 2024-02-22T18:00:00Z
    string iso8601Time = utcTime.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
    string plaintextAccessCode = lowercaseID + iso8601Time;
    byte[] utf8Bytes = Encoding.UTF8.GetBytes(plaintextAccessCode);
    byte[] hashedBytes = SHA256.HashData(utf8Bytes);
    string base64String = Convert.ToBase64String(hashedBytes);
    return base64String;
  }
}


