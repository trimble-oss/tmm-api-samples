// Top-level build file where you can add configuration options common to all sub-projects/modules.
plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.android) apply false
    alias(libs.plugins.kotlin.serialization) apply false
}

subprojects {
    configurations.configureEach {
        resolutionStrategy {
            force("org.bouncycastle:bcprov-jdk18on:1.85")
            force("org.bouncycastle:bcpkix-jdk18on:1.85")
            force("org.bouncycastle:bcutil-jdk18on:1.85")
            // CVE-2025-48924 (lint-gradle -> commons-lang3 3.16.0)
            force("org.apache.commons:commons-lang3:3.18.0")
            // CVE-2020-13956 (lint-gradle -> httpclient 4.5.6)
            force("org.apache.httpcomponents:httpclient:4.5.14")
        }
    }
}
