// Top-level build file where you can add configuration options common to all sub-projects/modules.
plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.android) apply false
}

subprojects {
    configurations.configureEach {
        resolutionStrategy {
            force("io.netty:netty-codec-http2:4.1.129.Final")
            force("io.netty:netty-codec-http:4.1.129.Final")
            force("commons-io:commons-io:2.21.0")
            force("com.google.protobuf:protobuf-java:4.33.1")
            force("com.google.protobuf:protobuf-kotlin:4.33.1")
            force("org.bouncycastle:bcprov-jdk18on:1.84")
            force("org.bouncycastle:bcpkix-jdk18on:1.84")
            force("org.apache.commons:commons-lang3:3.18.0")
            force("org.apache.httpcomponents:httpclient:4.5.13")
        }
    }
}
