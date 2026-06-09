package com.www4r4e.compose.data.mock

import com.www4r4e.compose.data.model.GitHubUser
import com.www4r4e.compose.data.model.Repository
import kotlin.math.absoluteValue

data class TrendingRepo(
    val repo: Repository,
    val todayStars: Int
)

object MockTrending {
    val daily: List<TrendingRepo> = listOf(
        trending("Kotlin/awesome-kotlin", "A curated list of awesome Kotlin resources", "kotlin", 5200, 1200, 45),
        trending("google/gemma-cpp", "C++ inference for Google's Gemma models", "c-plus-plus", 3400, 380, 120),
        trending("astral-sh/ruff", "An extremely fast Python linter written in Rust", "rust", 36000, 1200, 89),
        trending("nccloud/nc", "Graph-based AI workflow engine", "typescript", 8900, 560, 234),
        trending("tauri-apps/tauri", "Build smaller, faster, and more secure desktop applications", "rust", 87000, 2600, 56),
        trending("ladybird/ladybird", "Truly independent web browser", "c-plus-plus", 28000, 1200, 178),
        trending("surrealdb/surrealdb", "A scalable, distributed, collaborative document database", "rust", 29000, 760, 34),
        trending("oven-sh/bun", "Incredibly fast JavaScript runtime", "zig", 76000, 2800, 67),
        trending("zed-industries/zed", "Code at the speed of thought", "rust", 54000, 1800, 92),
        trending("DenverCoder1/readme-typing-svg", "Dynamic typing SVG for GitHub READMEs", "typescript", 1200, 340, 15),
    )

    val weekly: List<TrendingRepo> = daily.map { it.copy(todayStars = it.todayStars * 5) }

    val monthly: List<TrendingRepo> = daily.map { it.copy(todayStars = it.todayStars * 20) }

    private fun trending(
        fullName: String,
        desc: String,
        lang: String,
        stars: Int,
        forks: Int,
        todayDelta: Int
    ): TrendingRepo {
        val parts = fullName.split("/")
        val owner = GitHubUser(
            login = parts[0], id = parts[0].hashCode().toLong(),
            avatarUrl = "https://avatars.githubusercontent.com/u/${parts[0].hashCode().toLong().absoluteValue % 100000}",
            htmlUrl = "https://github.com/${parts[0]}"
        )
        return TrendingRepo(
            repo = Repository(
                id = (10000L..99999L).random(),
                name = parts[1],
                fullName = fullName,
                owner = owner,
                htmlUrl = "https://github.com/$fullName",
                description = desc,
                createdAt = "2023-01-15T10:00:00Z",
                updatedAt = "2025-12-20T10:00:00Z",
                stargazersCount = stars,
                language = lang,
                forksCount = forks,
                topics = listOf(lang.replace("_", "-"))
            ),
            todayStars = todayDelta
        )
    }
}
