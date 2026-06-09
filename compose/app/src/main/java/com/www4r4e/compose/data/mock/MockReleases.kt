package com.www4r4e.compose.data.mock

import com.www4r4e.compose.data.model.Release
import com.www4r4e.compose.data.model.ReleaseAsset

object MockReleases {
    val releases: Map<String, List<Release>> = mapOf(
        "Kotlin/kotlin" to listOf(
            Release(1, "v2.1.0", name = "Kotlin 2.1.0", body = "## What's New\n\n### Compose Multiplatform\n- Improved performance for iOS targets\n- New layout primitives\n\n### K2 Compiler\n- Faster compilation times\n- Better type inference\n\n### Gradle Support\n- Gradle 8.9+ compatibility", publishedAt = "2025-11-15T10:00:00Z", htmlUrl = "https://github.com/Kotlin/kotlin/releases/v2.1.0", createdAt = "2025-11-15T10:00:00Z"),
            Release(2, "v2.0.21", name = "Kotlin 2.0.21", body = "Bug-fix release for Kotlin 2.0.20", publishedAt = "2025-08-10T10:00:00Z", htmlUrl = "https://github.com/Kotlin/kotlin/releases/v2.0.21", createdAt = "2025-08-10T10:00:00Z"),
            Release(3, "v2.0.20", name = "Kotlin 2.0.20", body = "## What's New\n\n- K2 compiler now stable for all target backends\n- Improved Compose compiler plugin\n- New `@Continuation` annotation", publishedAt = "2025-06-05T10:00:00Z", htmlUrl = "https://github.com/Kotlin/kotlin/releases/v2.0.20", createdAt = "2025-06-05T10:00:00Z"),
        ),
        "square/okhttp" to listOf(
            Release(10, "5.0.0-alpha.14", name = "OkHttp 5.0.0-alpha14", body = "### Changes\n\n- Fix: WebSocket frame reading on JDK9+\n- New: coroutine-based API preview\n- Upgrade: Kotlin 2.1.0", publishedAt = "2025-11-01T10:00:00Z", htmlUrl = "https://github.com/square/okhttp/releases/5.0.0-alpha.14", createdAt = "2025-11-01T10:00:00Z"),
            Release(11, "4.12.0", name = "OkHttp 4.12.0", body = "Stable release with bug fixes", publishedAt = "2025-03-20T10:00:00Z", htmlUrl = "https://github.com/square/okhttp/releases/4.12.0", createdAt = "2025-03-20T10:00:00Z"),
        ),
        "android/nowinandroid" to listOf(
            Release(20, "v1.5.0", name = "Now in Android v1.5", body = "## What's New\n\n- Updated to Compose BOM 2025.01.00\n- Adaptive layout support for tablets\n- New Onboarding flow\n- Dynamic color refinements", publishedAt = "2025-12-10T10:00:00Z", htmlUrl = "https://github.com/android/nowinandroid/releases/v1.5.0", createdAt = "2025-12-10T10:00:00Z", assets = listOf(
                ReleaseAsset(1, "app-release.apk", "application/vnd.android.package-archive", 25000000, "https://github.com/android/nowinandroid/releases/download/v1.5.0/app-release.apk", "2025-12-10T10:00:00Z", "2025-12-10T10:00:00Z")
            )),
            Release(21, "v1.4.0", name = "Now in Android v1.4", body = "Bug fixes and performance improvements", publishedAt = "2025-09-01T10:00:00Z", htmlUrl = "https://github.com/android/nowinandroid/releases/v1.4.0", createdAt = "2025-09-01T10:00:00Z"),
        ),
        "facebook/react" to listOf(
            Release(30, "v19.0.0", name = "React 19", body = "## React 19\n\n### New Features\n\n- Server Components (stable)\n- Actions API\n- New Hooks: `use()` API\n- Improved Suspense\n- Concurrent features GA", publishedAt = "2025-12-05T10:00:00Z", htmlUrl = "https://github.com/facebook/react/releases/v19.0.0", createdAt = "2025-12-05T10:00:00Z"),
        ),
        "microsoft/vscode" to listOf(
            Release(40, "1.96.0", name = "VS Code 1.96", body = "### Highlights\n\n- New AI-powered code suggestions\n- Improved terminal performance\n- GitHub Copilot integration updates", publishedAt = "2025-12-15T10:00:00Z", htmlUrl = "https://github.com/microsoft/vscode/releases/1.96.0", createdAt = "2025-12-15T10:00:00Z"),
        ),
    )
}
