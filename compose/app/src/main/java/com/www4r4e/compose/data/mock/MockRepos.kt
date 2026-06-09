package com.www4r4e.compose.data.mock

import com.www4r4e.compose.data.model.GitHubUser
import com.www4r4e.compose.data.model.License
import com.www4r4e.compose.data.model.Repository

object MockRepos {
    private val owners = listOf(
        GitHubUser(login = "jetbrains", id = 878437, avatarUrl = "https://avatars.githubusercontent.com/u/878437", htmlUrl = "https://github.com/jetbrains", name = "JetBrains"),
        GitHubUser(login = "google", id = 1342004, avatarUrl = "https://avatars.githubusercontent.com/u/1342004", htmlUrl = "https://github.com/google", name = "Google"),
        GitHubUser(login = "microsoft", id = 6154722, avatarUrl = "https://avatars.githubusercontent.com/u/6154722", htmlUrl = "https://github.com/microsoft", name = "Microsoft"),
        GitHubUser(login = "apache", id = 47359, avatarUrl = "https://avatars.githubusercontent.com/u/47359", htmlUrl = "https://github.com/apache", name = "Apache"),
        GitHubUser(login = "facebook", id = 69631, avatarUrl = "https://avatars.githubusercontent.com/u/69631", htmlUrl = "https://github.com/facebook", name = "Facebook"),
        GitHubUser(login = "android", id = 32689522, avatarUrl = "https://avatars.githubusercontent.com/u/32689522", htmlUrl = "https://github.com/android", name = "Android"),
        GitHubUser(login = "square", id = 82592, avatarUrl = "https://avatars.githubusercontent.com/u/82592", htmlUrl = "https://github.com/square", name = "Square"),
        GitHubUser(login = "Kotlin", id = 1866182, avatarUrl = "https://avatars.githubusercontent.com/u/1866182", htmlUrl = "https://github.com/Kotlin", name = "Kotlin"),
        GitHubUser(login = "spring-projects", id = 317776, avatarUrl = "https://avatars.githubusercontent.com/u/317776", htmlUrl = "https://github.com/spring-projects", name = "Spring"),
        GitHubUser(login = "rust-lang", id = 5430905, avatarUrl = "https://avatars.githubusercontent.com/u/5430905", htmlUrl = "https://github.com/rust-lang", name = "Rust"),
    )

    private val repoDefs = listOf(
        RepoDef("Kotlin", "kotlin", "Kotlin", "The Kotlin Programming Language", "kotlin", 50200, 5800, topics = listOf("kotlin", "language", "compiler", "jvm")),
        RepoDef("jetbrains", "compose-multiplatform", "Kotlin", "Compose Multiplatform, a modern UI framework for Kotlin", "kotlin", 16500, 1200, topics = listOf("compose", "multiplatform", "ui", "kotlin")),
        RepoDef("google", "compose-samples", "Kotlin", "Official Jetpack Compose samples", "kotlin", 20500, 4900, topics = listOf("android", "compose", "samples", "jetpack")),
        RepoDef("android", "compose-samples", "Kotlin", "Official Android Jetpack Compose samples from Google", "kotlin", 20500, 4900, topics = listOf("android", "compose", "jetpack", "samples")),
        RepoDef("square", "okhttp", "Kotlin", "Square's HTTP client for Kotlin and Java", "kotlin", 46000, 9100, topics = listOf("http", "client", "android", "kotlin")),
        RepoDef("google", "gson", "Java", "A Java serialization/deserialization library", "java", 23500, 4200, topics = listOf("json", "java", "serialization", "google")),
        RepoDef("apache", "kafka", "Java", "Apache Kafka distributed event streaming platform", "java", 29500, 14100, fork = true, topics = listOf("kafka", "streaming", "distributed", "pubsub")),
        RepoDef("spring-projects", "spring-boot", "Java", "Spring Boot makes it easy to create Spring-powered apps", "java", 76000, 40600, topics = listOf("spring", "java", "framework", "microservices")),
        RepoDef("microsoft", "vscode", "TypeScript", "Visual Studio Code - Open source code editor", "typescript", 166000, 29700, topics = listOf("editor", "code", "typescript", "microsoft")),
        RepoDef("facebook", "react", "JavaScript", "A declarative, efficient JavaScript library for building UIs", "javascript", 231000, 47300, topics = listOf("react", "javascript", "ui", "frontend")),
        RepoDef("microsoft", "TypeScript", "TypeScript", "TypeScript is a superset of JavaScript", "typescript", 102000, 12600, topics = listOf("typescript", "language", "compiler", "javascript")),
        RepoDef("rust-lang", "rust", "Rust", "Empowering everyone to build reliable and efficient software", "rust", 100000, 12900, topics = listOf("rust", "language", "compiler", "systems")),
        RepoDef("google", "android-architecture", "Kotlin", "A collection of samples discussing different Android architectures", "kotlin", 21500, 4100, topics = listOf("android", "architecture", "mvvm", "clean-architecture")),
        RepoDef("google", "codelab-android-compose", "Kotlin", "Jetpack Compose Codelabs", "kotlin", 1400, 420, topics = listOf("compose", "codelab", "android", "tutorial")),
        RepoDef("Kotlin", "android", "Kotlin", "Kotlin Android ecosystem tools and libraries", "kotlin", 2800, 310, topics = listOf("kotlin", "android", "libraries")),
        RepoDef("microsoft", "terminal", "C++", "The new Windows Terminal", "c-plus-plus", 96000, 8400, topics = listOf("terminal", "windows", "console", "microsoft")),
        RepoDef("android", "nowinandroid", "Kotlin", "A fully functional Android app built with Jetpack Compose", "kotlin", 17500, 3100, topics = listOf("android", "compose", "material3", "architecture")),
    )

    val repos: List<Repository> = repoDefs.mapIndexed { index, def ->
        val owner = owners.find { it.login == def.ownerLogin } ?: owners.first()
        Repository(
            id = 1000L + index,
            name = def.repoName,
            fullName = "${def.ownerLogin}/${def.repoName}",
            owner = owner,
            htmlUrl = "https://github.com/${def.ownerLogin}/${def.repoName}",
            description = def.description,
            fork = def.fork,
            createdAt = "2024-0${(index % 9) + 1}-15T10:00:00Z",
            updatedAt = "2025-0${(index % 9) + 1}-20T10:00:00Z",
            stargazersCount = def.stars,
            watchersCount = def.stars / 10,
            language = def.language,
            forksCount = def.forks,
            topics = def.topics,
            license = License("mit", "MIT License", "MIT"),
            defaultBranch = "main"
        )
    }

    private data class RepoDef(
        val ownerLogin: String,
        val repoName: String,
        val language: String,
        val description: String,
        val langTag: String,
        val stars: Int,
        val forks: Int,
        val fork: Boolean = false,
        val topics: List<String> = emptyList()
    )
}
