using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MauiSample.AccessCode;

static public class AccessCodeV2
{
  private static RSA? _publicKey;

  public static void SetPublicKey(string jwkJson)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(jwkJson);

    using JsonDocument document = JsonDocument.Parse(jwkJson);
    JsonElement root = document.RootElement;

    if (root.GetProperty("kty").GetString() != "RSA")
    {
      throw new ArgumentException("Only RSA JWK keys are supported.", nameof(jwkJson));
    }

    if(!root.TryGetProperty("n", out JsonElement n) || !root.TryGetProperty("e", out JsonElement e))
    {
      throw new ArgumentException("n and e properties are required.", nameof(jwkJson));
    }
    
    if(n.GetString() is not string modulusString || e.GetString() is not string exponentString)
    {
      throw new ArgumentException("n and e properties must be strings.", nameof(jwkJson));
    }

    byte[] modulus = Base64UrlDecode(modulusString);
    byte[] exponent = Base64UrlDecode(exponentString);

    var rsa = RSA.Create();
    rsa.ImportParameters(new RSAParameters
    {
      Modulus = modulus,
      Exponent = exponent,
    });

    _publicKey?.Dispose();
    _publicKey = rsa;
  }

  private static byte[] Base64UrlDecode(string base64Url)
  {
    string base64 = base64Url.Replace('-', '+').Replace('_', '/');
    int padding = (4 - base64.Length % 4) % 4;
    return Convert.FromBase64String(base64.PadRight(base64.Length + padding, '='));
  }

  public static string Generate(Guid appID, DateTime utcTime)
  {
    if(_publicKey is null)
    {
      throw new InvalidOperationException("public key not set");
    }

    string lowercaseID = appID.ToString("D").ToLowerInvariant();
    // Format utcTime as an ISO8601 compliant string, like this: 2024-02-22T18:00:00Z
    string iso8601Time = utcTime.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
    string plaintextAccessCode = lowercaseID + " " + iso8601Time;
    byte[] utf8Bytes = Encoding.UTF8.GetBytes(plaintextAccessCode);
    byte[] encryptedBytes = _publicKey.Encrypt(utf8Bytes, RSAEncryptionPadding.OaepSHA256);
    string base64String = Convert.ToBase64String(encryptedBytes);
    return base64String;
  }
}
