// Top-level build file where you can add configuration options common to all sub-projects/modules.
buildscript {
    configurations.classpath {
        resolutionStrategy {
            // lint-gradle on the plugin classpath is outside subproject resolution.
            // CVE-2026-97873 is fixed in bcprov 1.86 (affected range is < 1.86).
            // 1.86 is the newest bcprov-jdk18on on Maven Central.
            force("org.bouncycastle:bcprov-jdk18on:1.86")
            force("org.bouncycastle:bcpkix-jdk18on:1.86")
            force("org.bouncycastle:bcutil-jdk18on:1.86")
            force("org.apache.commons:commons-lang3:3.18.0")
            force("org.apache.httpcomponents:httpclient:4.5.14")
            // CVE-2026-102554 (guava through 33.7.1). Lint requests the -jre artifact.
            force("com.google.guava:guava:33.7.2-jre")
            // CVE-2026-110257, CVE-2026-110258, CVE-2026-110259, CVE-2026-110260,
            // CVE-2026-110261, CVE-2026-110262, CVE-2026-110263, CVE-2026-110270,
            // CVE-2026-110591 (asm before 9.11). 9.11.1 also fixes the 9.11 regression.
            force("org.ow2.asm:asm:9.11.1")
            force("org.ow2.asm:asm-commons:9.11.1")
            force("org.ow2.asm:asm-tree:9.11.1")
            force("org.ow2.asm:asm-analysis:9.11.1")
            force("org.ow2.asm:asm-util:9.11.1")
        }
    }
}

plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.android) apply false
    alias(libs.plugins.kotlin.serialization) apply false
}

subprojects {
    configurations.configureEach {
        resolutionStrategy {
            // CVE-2026-71888, CVE-2026-71889, CVE-2026-71891, CVE-2026-71892,
            // CVE-2026-18036, CVE-2026-18040, CVE-2026-97873, CVE-2026-17508
            // (lint-gradle -> bouncycastle; CVE-2026-97873 fixed in 1.86, range < 1.86)
            force("org.bouncycastle:bcprov-jdk18on:1.86")
            force("org.bouncycastle:bcpkix-jdk18on:1.86")
            force("org.bouncycastle:bcutil-jdk18on:1.86")
            // CVE-2025-48924 (lint-gradle -> commons-lang3 3.16.0)
            force("org.apache.commons:commons-lang3:3.18.0")
            // CVE-2020-13956 (lint-gradle -> httpclient 4.5.6)
            force("org.apache.httpcomponents:httpclient:4.5.14")
            // CVE-2026-102554 (lint-typedef-remover -> guava 33.4.0-jre)
            force("com.google.guava:guava:33.7.2-jre")
            // CVE-2026-110257, CVE-2026-110258, CVE-2026-110259, CVE-2026-110260,
            // CVE-2026-110261, CVE-2026-110262, CVE-2026-110263, CVE-2026-110270,
            // CVE-2026-110591 (lint -> asm 9.9; fixed in 9.11, use 9.11.1)
            force("org.ow2.asm:asm:9.11.1")
            force("org.ow2.asm:asm-commons:9.11.1")
            force("org.ow2.asm:asm-tree:9.11.1")
            force("org.ow2.asm:asm-analysis:9.11.1")
            force("org.ow2.asm:asm-util:9.11.1")
        }
    }
}
