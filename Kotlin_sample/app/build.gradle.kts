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
  implementation(libs.kotlinx.serialization.json)
}

configurations.all {
    resolutionStrategy {
        force("io.netty:netty-codec-http2:4.1.129.Final")
        force("io.netty:netty-codec-http:4.1.129.Final")
        force("commons-io:commons-io:2.21.0")
        force("com.google.protobuf:protobuf-java:4.33.1")
        force("com.google.protobuf:protobuf-kotlin:4.33.1")
    }
}