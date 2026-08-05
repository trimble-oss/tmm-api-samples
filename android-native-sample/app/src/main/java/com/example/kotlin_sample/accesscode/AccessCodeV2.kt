package com.example.kotlin_sample.accesscode

import org.json.JSONObject
import java.math.BigInteger
import java.security.KeyFactory
import java.security.interfaces.RSAPublicKey
import java.security.spec.MGF1ParameterSpec
import java.security.spec.RSAPublicKeySpec
import java.time.Instant
import java.time.ZoneOffset
import java.time.format.DateTimeFormatter
import java.util.Base64
import java.util.UUID
import javax.crypto.Cipher
import javax.crypto.spec.OAEPParameterSpec
import javax.crypto.spec.PSource

object AccessCodeV2 {
  private val iso8601Formatter: DateTimeFormatter =
    DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ssX")
      .withZone(ZoneOffset.UTC)

  @Volatile
  private var publicKey: RSAPublicKey? = null

  fun setPublicKey(jwkJson: String) {
    require(jwkJson.isNotBlank()) { "jwkJson must not be blank" }

    val root = JSONObject(jwkJson)
    require(root.getString("kty") == "RSA") { "Only RSA JWK keys are supported." }
    require(root.has("n") && root.has("e")) { "n and e properties are required." }

    val modulus = base64UrlDecode(root.getString("n"))
    val exponent = base64UrlDecode(root.getString("e"))
    val keySpec = RSAPublicKeySpec(BigInteger(1, modulus), BigInteger(1, exponent))
    publicKey = KeyFactory.getInstance("RSA").generatePublic(keySpec) as RSAPublicKey
  }

  fun generate(appId: UUID, utcTime: Instant): String {
    val key = publicKey ?: throw IllegalStateException("public key not set")

    val lowercaseId = appId.toString().lowercase()
    val iso8601Time = iso8601Formatter.format(utcTime)
    val plaintextAccessCode = "$lowercaseId $iso8601Time"
    val utf8Bytes = plaintextAccessCode.toByteArray(Charsets.UTF_8)

    val cipher = Cipher.getInstance("RSA/ECB/OAEPWithSHA-256AndMGF1Padding")
    val oaepParams = OAEPParameterSpec(
      "SHA-256",
      "MGF1",
      MGF1ParameterSpec.SHA256,
      PSource.PSpecified.DEFAULT,
    )
    cipher.init(Cipher.ENCRYPT_MODE, key, oaepParams)
    return Base64.getEncoder().encodeToString(cipher.doFinal(utf8Bytes))
  }

  private fun base64UrlDecode(base64Url: String): ByteArray {
    var base64 = base64Url.replace('-', '+').replace('_', '/')
    val padding = (4 - base64.length % 4) % 4
    base64 += "=".repeat(padding)
    return Base64.getDecoder().decode(base64)
  }
}
