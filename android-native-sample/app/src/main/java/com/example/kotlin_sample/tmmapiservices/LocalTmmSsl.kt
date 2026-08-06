package com.example.kotlin_sample.tmmapiservices

import java.security.KeyStore
import java.security.SecureRandom
import java.security.cert.CertificateException
import java.security.cert.X509Certificate
import javax.net.ssl.HttpsURLConnection
import javax.net.ssl.SSLContext
import javax.net.ssl.SSLSocketFactory
import javax.net.ssl.TrustManagerFactory
import javax.net.ssl.X509TrustManager
import okhttp3.OkHttpClient

/**
 * The local TMM API certificate chain is not in Android's system trust store.
 * Allow chain trust failures for known local hosts; hostname checks remain enforced.
 */
internal object LocalTmmSsl {
  private const val LOCAL_TMM_CERT_CN = "tmm-api-local.fieldsystems.trimble.com"

  private val localServerHosts = setOf(
    "localhost",
    "127.0.0.1",
    LOCAL_TMM_CERT_CN,
  )

  fun applyTo(builder: OkHttpClient.Builder): OkHttpClient.Builder {
    val trustManager = createLocalAwareTrustManager()
    val sslContext = SSLContext.getInstance("TLS").apply {
      init(null, arrayOf(trustManager), SecureRandom())
    }
    val defaultHostnameVerifier = HttpsURLConnection.getDefaultHostnameVerifier()
    return builder
      .sslSocketFactory(sslContext.socketFactory as SSLSocketFactory, trustManager)
      .hostnameVerifier { hostname, session ->
        hostname in localServerHosts && defaultHostnameVerifier.verify(hostname, session)
      }
  }

  private fun createLocalAwareTrustManager(): X509TrustManager {
    val factory = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm())
    factory.init(null as KeyStore?)
    val defaultTrustManager = factory.trustManagers
      .filterIsInstance<X509TrustManager>()
      .first()

    return object : X509TrustManager {
      override fun checkClientTrusted(chain: Array<X509Certificate>, authType: String) {
        defaultTrustManager.checkClientTrusted(chain, authType)
      }

      override fun checkServerTrusted(chain: Array<X509Certificate>, authType: String) {
        try {
          defaultTrustManager.checkServerTrusted(chain, authType)
        } catch (ex: CertificateException) {
          if (!chainContainsLocalTmmCertificate(chain)) {
            throw ex
          }
          // Allow untrusted / incomplete chains for the local TMM API certificate.
          // Hostname verification is still performed by OkHttp.
        }
      }

      override fun getAcceptedIssuers(): Array<X509Certificate> =
        defaultTrustManager.acceptedIssuers
    }
  }

  private fun chainContainsLocalTmmCertificate(chain: Array<X509Certificate>): Boolean =
    chain.any { certificate -> getCommonName(certificate) == LOCAL_TMM_CERT_CN }

  private val commonNamePattern = Regex("""(?:^|,)CN=([^,]+)""", RegexOption.IGNORE_CASE)

  private fun getCommonName(certificate: X509Certificate): String? =
    commonNamePattern
      .find(certificate.subjectX500Principal.name)
      ?.groupValues
      ?.get(1)
      ?.trim()
}
