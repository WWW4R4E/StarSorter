package com.www4r4e.compose.navigation

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Notifications
import androidx.compose.material.icons.filled.Star
import androidx.compose.material.icons.outlined.Notifications
import androidx.compose.material.icons.outlined.Star
import androidx.compose.ui.graphics.vector.ImageVector
import com.www4r4e.compose.ui.components.GitIcons

sealed class Screen(
    val route: String,
    val label: String = "",
    val selectedIcon: ImageVector? = null,
    val unselectedIcon: ImageVector? = null,
) {
    data object Stars : Screen("stars", "Stars", Icons.Filled.Star, Icons.Outlined.Star)
    data object Explore : Screen("explore", "Explore", GitIcons.Explore, GitIcons.Explore)
    data object Notifications : Screen("notifications", "Notifications", Icons.Filled.Notifications, Icons.Outlined.Notifications)
    data object Profile : Screen("profile")
    data object Settings : Screen("settings")
    data object StarDetail : Screen("star_detail/{owner}/{repo}") {
        fun createRoute(owner: String, repo: String) = "star_detail/$owner/$repo"
    }
    data object Search : Screen("search")
    data object CategoryResult : Screen("category_result/{category}") {
        fun createRoute(category: String) = "category_result/$category"
    }

    companion object {
        val bottomNavItems = listOf(Stars, Explore, Notifications)
    }
}
