package com.www4r4e.compose.data.mock

import com.www4r4e.compose.data.model.GitHubUser
import com.www4r4e.compose.data.model.RateLimit
import com.www4r4e.compose.data.model.RateLimitItem
import com.www4r4e.compose.data.model.RateLimitResources

object MockUser {
    val currentUser = GitHubUser(
        login = "www4r4e",
        id = 12345678,
        avatarUrl = "https://avatars.githubusercontent.com/u/12345678",
        htmlUrl = "https://github.com/www4r4e",
        name = "R4E Developer",
        company = "@WWW4R4E",
        blog = "https://opencode.ai",
        location = "Earth",
        email = "dev@example.com",
        bio = "Building the future of code. Jetpack Compose enthusiast. Open source contributor.",
        twitterUsername = "www4r4e",
        publicRepos = 42,
        publicGists = 12,
        followers = 256,
        following = 128,
        createdAt = "2020-03-15T10:00:00Z",
        updatedAt = "2025-12-20T10:00:00Z"
    )

    val rateLimit = RateLimit(
        resources = RateLimitResources(
            core = RateLimitItem(limit = 5000, remaining = 4876, reset = 1704100000, used = 124),
            search = RateLimitItem(limit = 30, remaining = 27, reset = 1704100000, used = 3),
            graphql = RateLimitItem(limit = 5000, remaining = 5000, reset = 1704100000, used = 0)
        )
    )
}
