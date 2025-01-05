import java.util.Properties

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
}

android {
    namespace = "com.example.kotlin_sample"
  compileSdk = 35

  defaultConfig {
        applicationId = "com.example.kotlin_sample"
      minSdk = 26
      targetSdk = 35
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    kotlinOptions {
        jvmTarget = "11"
    }
  buildTypes {
    debug {
      val sensitiveDataFile = rootProject.file("sensitiveData.properties")
      val sensitiveData = Properties().apply {
        load(sensitiveDataFile.inputStream())
      }
      val appID: String = sensitiveData["appID"].toString()
      buildConfigField("String", "appID", appID)
    }
  }
  buildFeatures {
    buildConfig = true
  }
  buildToolsVersion = "35.0.0"
}

dependencies {

    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.appcompat)
    implementation(libs.material)
    implementation(libs.androidx.activity)
    implementation(libs.androidx.constraintlayout)
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    implementation(libs.ktor.client.core)
    implementation(libs.ktor.client.cio)
    implementation (libs.converter.gson)
    implementation (libs.okhttp)
}
