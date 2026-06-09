package com.www4r4e.compose.data.model

import kotlinx.serialization.Serializable

@Serializable
data class GitHubUser(
    val login: String,
    val id: Long,
    val nodeId: String? = null,
    val avatarUrl: String? = null,
    val htmlUrl: String? = null,
    val name: String? = null,
    val company: String? = null,
    val blog: String? = null,
    val location: String? = null,
    val email: String? = null,
    val bio: String? = null,
    val twitterUsername: String? = null,
    val publicRepos: Int? = null,
    val publicGists: Int? = null,
    val followers: Int? = null,
    val following: Int? = null,
    val createdAt: String? = null,
    val updatedAt: String? = null
)

@Serializable
data class Repository(
    val id: Long,
    val name: String,
    val fullName: String,
    val private: Boolean = false,
    val owner: GitHubUser,
    val htmlUrl: String,
    val description: String? = null,
    val fork: Boolean = false,
    val createdAt: String,
    val updatedAt: String,
    val pushedAt: String? = null,
    val homepage: String? = null,
    val size: Long? = null,
    val stargazersCount: Int = 0,
    val watchersCount: Int = 0,
    val language: String? = null,
    val forksCount: Int = 0,
    val openIssuesCount: Int = 0,
    val topics: List<String>? = null,
    val visibility: String? = null,
    val defaultBranch: String = "main",
    val license: License? = null
)

@Serializable
data class License(
    val key: String,
    val name: String,
    val spdxId: String? = null,
    val url: String? = null,
    val nodeId: String? = null
)

@Serializable
data class Release(
    val id: Long,
    val tagName: String,
    val targetCommitish: String = "main",
    val name: String? = null,
    val body: String? = null,
    val draft: Boolean = false,
    val prerelease: Boolean = false,
    val createdAt: String,
    val publishedAt: String? = null,
    val htmlUrl: String,
    val assets: List<ReleaseAsset> = emptyList(),
    val tarballUrl: String? = null,
    val zipballUrl: String? = null
)

@Serializable
data class ReleaseAsset(
    val id: Long,
    val name: String,
    val contentType: String,
    val size: Long,
    val browserDownloadUrl: String,
    val createdAt: String,
    val updatedAt: String
)

@Serializable
data class SearchResults(
    val totalCount: Long,
    val incompleteResults: Boolean = false,
    val items: List<Repository> = emptyList()
)

@Serializable
data class RateLimit(
    val resources: RateLimitResources
)

@Serializable
data class RateLimitResources(
    val core: RateLimitItem,
    val search: RateLimitItem,
    val graphql: RateLimitItem
)

@Serializable
data class RateLimitItem(
    val limit: Int,
    val remaining: Int,
    val reset: Long,
    val used: Int
)

@Serializable
data class Notification(
    val id: String,
    val repoFullName: String,
    val repoAvatarUrl: String?,
    val type: NotificationType,
    val title: String,
    val body: String?,
    val url: String,
    val timestamp: String,
    val isUnread: Boolean = true
)

enum class NotificationType {
    NEW_RELEASE,
    NEW_STAR,
    NEW_FORK,
    NEW_ISSUE,
    NEW_PR
}

enum class DiscoveryChannel(val label: String, val icon: String) {
    MOST_STARS("Most Stars", "whatshot"),
    MOST_FORKS("Most Forks", "call_split"),
    HOT_RELEASES("Hot Releases", "rocket_launch"),
    MOST_POPULAR("Most Popular", "emoji_events"),
    TRENDING("Trending", "trending_up"),
    AI("AI / ML", "psychology"),
    WEB("Web", "language"),
    MOBILE("Mobile", "smartphone"),
    DATABASE("Database", "storage"),
    DEVTOOLS("DevTools", "build"),
    GAME("Game", "sports_esports"),
    DEVELOPERS("Developers", "groups")
}

@Serializable
data class StarList(
    val id: String,
    val name: String,
    val description: String? = null,
    val repos: List<Repository> = emptyList(),
    val createdAt: String,
    val updatedAt: String,
    val isPublic: Boolean = true
)

@Serializable
data class FilterOptions(
    val languages: List<String> = emptyList(),
    val selectedLanguages: Set<String> = emptySet(),
    val topics: List<String> = emptyList(),
    val selectedTopics: Set<String> = emptySet(),
    val sortBy: SortOption = SortOption.STARGAZERS,
    val sortOrder: SortOrder = SortOrder.DESCENDING,
    val showForks: Boolean = true,
    val minStars: Int? = null,
    val maxStars: Int? = null
)

enum class SortOption(val label: String) {
    STARGAZERS("Stars"),
    FORKS("Forks"),
    NAME("Name"),
    UPDATED("Last Updated"),
    CREATED("Created")
}

enum class SortOrder(val label: String) {
    ASCENDING("Ascending"),
    DESCENDING("Descending")
}

/**
 * 用户设置和偏好
 */
@kotlinx.serialization.Serializable
data class UserSettings(
    val theme: String = "system", // "light", "dark", "system"
    val dynamicColor: Boolean = true,
    val autoRefreshEnabled: Boolean = true,
    val refreshIntervalHours: Int = 24,
    val imageCacheDays: Int = 30,
    val maxImageCacheMB: Int = 100,
    val apiCacheHours: Int = 24,
    val notificationsEnabled: Boolean = true,
    val language: String = "en",
    val lastAppVersion: String? = null
)
