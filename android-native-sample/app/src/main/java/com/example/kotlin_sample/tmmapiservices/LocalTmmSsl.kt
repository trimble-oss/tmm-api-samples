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
  private val localServerHosts = setOf(
    "localhost",
    "127.0.0.1",
    "tmm-api-local.fieldsystems.trimble.com",
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
        } catch (_: CertificateException) {
          // Allow untrusted / incomplete chains used by the local TMM API.
          // Hostname verification is still performed by OkHttp.
        }
      }

      override fun getAcceptedIssuers(): Array<X509Certificate> =
        defaultTrustManager.acceptedIssuers
    }
  }
}
