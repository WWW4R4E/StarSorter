package com.www4r4e.compose.data.mock

import com.www4r4e.compose.data.model.Notification
import com.www4r4e.compose.data.model.NotificationType

object MockNotifications {
    val notifications = listOf(
        Notification("1", "Kotlin/kotlin", "https://avatars.githubusercontent.com/u/1866182", NotificationType.NEW_RELEASE, "Kotlin 2.1.0 released", "## What's New\n\n- Compose Multiplatform improvements\n- K2 compiler enhancements\n- Better Gradle support", "https://github.com/Kotlin/kotlin/releases/v2.1.0", "2025-12-15T10:00:00Z", false),
        Notification("2", "android/nowinandroid", "https://avatars.githubusercontent.com/u/32689522", NotificationType.NEW_RELEASE, "Now in Android v1.5 released", "Updated to Compose BOM 2025.01.00 with adaptive layout support for tablets.", "https://github.com/android/nowinandroid/releases/v1.5.0", "2025-12-10T08:30:00Z", false),
        Notification("3", "facebook/react", "https://avatars.githubusercontent.com/u/69631", NotificationType.NEW_RELEASE, "React 19 is here!", "React 19 brings Server Components (stable), Actions API, new `use()` hook, and concurrent features GA.", "https://github.com/facebook/react/releases/v19.0.0", "2025-12-05T14:00:00Z", true),
        Notification("4", "microsoft/vscode", "https://avatars.githubusercontent.com/u/6154722", NotificationType.NEW_RELEASE, "VS Code 1.96 released", "New AI-powered code suggestions, improved terminal performance.", "https://github.com/microsoft/vscode/releases/1.96.0", "2025-12-15T16:45:00Z", true),
        Notification("5", "square/okhttp", "https://avatars.githubusercontent.com/u/82592", NotificationType.NEW_RELEASE, "OkHttp 5.0.0-alpha14", "WebSocket fixes, coroutine-based API preview, Kotlin 2.1.0 upgrade.", "https://github.com/square/okhttp/releases/5.0.0-alpha.14", "2025-11-01T09:00:00Z", true),
        Notification("6", "rust-lang/rust", "https://avatars.githubusercontent.com/u/5430905", NotificationType.NEW_STAR, "rust-lang/rust starred you!", "Your project caught their interest", "https://github.com/rust-lang/rust", "2025-12-14T11:20:00Z", false),
        Notification("7", "apache/kafka", "https://avatars.githubusercontent.com/u/47359", NotificationType.NEW_FORK, "Someone forked apache/kafka", "New fork created from your starred repo", "https://github.com/apache/kafka", "2025-12-13T22:15:00Z", true),
        Notification("8", "google/compose-samples", "https://avatars.githubusercontent.com/u/1342004", NotificationType.NEW_ISSUE, "New issue: Question about LazyColumn animations", "User asked about item animation in LazyColumn", "https://github.com/google/compose-samples/issues/1234", "2025-12-12T15:30:00Z", true),
    )
}
