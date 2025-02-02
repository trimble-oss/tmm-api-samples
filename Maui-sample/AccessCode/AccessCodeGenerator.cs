using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Maui_sample.AccessCode
{
    class AccessCodeGenerator
    {
    private string _accessCode;

    public string ID { get; private set; }
    public DateTime UtcTime { get; private set; }
    public string AccessCode => _accessCode ??= GenerateAccessCode();

    public AccessCodeGenerator(string id, DateTime utcTime)
    {
      if (utcTime.Kind != DateTimeKind.Utc)
      {
        throw new ArgumentException("utcTime must be of DateTimeKind.Utc kind", nameof(utcTime));
      }
      ID = id;
      UtcTime = utcTime;
    }

    public string GenerateAccessCode()
    {
      return GenerateAccessCode(ID, UtcTime);
    }

    public static string GenerateAccessCode(string appID, DateTime utcTime)
    {
      // Generates access code when trying to acccess receiver API or any API that requires it.
      string lowercaseID = appID.ToLowerInvariant();
      // Format utcTime as an ISO8601 compliant string, like this: 2024-02-22T18:00:00Z
      string iso8601Time = utcTime.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
      string plaintextAccessCode = lowercaseID + iso8601Time;
      byte[] utf8Bytes = Encoding.UTF8.GetBytes(plaintextAccessCode);
      byte[] hashedBytes = SHA256.HashData(utf8Bytes);
      string base64String = Convert.ToBase64String(hashedBytes);
      return base64String;
    }
  }
}
