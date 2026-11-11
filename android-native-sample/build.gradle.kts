// Top-level build file where you can add configuration options common to all sub-projects/modules.
buildscript {
    configurations.classpath {
        resolutionStrategy {
            // lint-gradle on the plugin classpath is outside subproject resolution.
            // CVE-2026-97873 (lint-gradle -> bcprov 1.86)
            force("org.bouncycastle:bcprov-jdk18on:1.87")
            force("org.bouncycastle:bcpkix-jdk18on:1.87")
            force("org.bouncycastle:bcutil-jdk18on:1.87")
            force("org.apache.commons:commons-lang3:3.18.0")
            force("org.apache.httpcomponents:httpclient:4.5.14")
            // CVE-2026-119078 (lint-gradle -> commons-codec 1.17.1)
            force("commons-codec:commons-codec:1.23.0")
            // CVE-2026-92877 (lint-gradle -> jaxb-runtime 2.3.2)
            force("org.glassfish.jaxb:jaxb-runtime:2.3.9")
            // CVE-2026-118042 (lint-gradle -> guava 33.7.2-jre)
            force("com.google.guava:guava:33.8.0-jre")
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
            // CVE-2026-97873 (lint-gradle -> bcprov 1.86)
            force("org.bouncycastle:bcprov-jdk18on:1.87")
            force("org.bouncycastle:bcpkix-jdk18on:1.87")
            force("org.bouncycastle:bcutil-jdk18on:1.87")
            // CVE-2025-48924 (lint-gradle -> commons-lang3 3.16.0)
            force("org.apache.commons:commons-lang3:3.18.0")
            // CVE-2020-13956 (lint-gradle -> httpclient 4.5.6)
            force("org.apache.httpcomponents:httpclient:4.5.14")
            // CVE-2026-119078 (lint-gradle -> commons-codec 1.17.1)
            force("commons-codec:commons-codec:1.23.0")
            // CVE-2026-92877 (lint-gradle -> jaxb-runtime 2.3.2)
            force("org.glassfish.jaxb:jaxb-runtime:2.3.9")
            // CVE-2026-118042 (lint-gradle -> guava 33.7.2-jre)
            force("com.google.guava:guava:33.8.0-jre")
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
