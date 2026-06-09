package com.www4r4e.compose.navigation

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.slideInVertically
import androidx.compose.animation.slideOutVertically
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.NavigationBarItemDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.www4r4e.compose.ui.detail.StarDetailScreen
import com.www4r4e.compose.ui.discover.CategoryResultScreen
import com.www4r4e.compose.ui.explore.ExploreScreen
import com.www4r4e.compose.ui.notifications.NotificationsScreen
import com.www4r4e.compose.ui.profile.ProfileScreen
import com.www4r4e.compose.ui.search.SearchScreen
import com.www4r4e.compose.ui.stars.StarsListScreen


@Composable
fun AppNavigation() {
    val navController = rememberNavController()
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    val showBottomBar = Screen.bottomNavItems.any { it.route == currentRoute }

    Scaffold(
        bottomBar = {
            AnimatedVisibility(
                visible = showBottomBar,
                enter = slideInVertically(initialOffsetY = { it }),
                exit = slideOutVertically(targetOffsetY = { it })
            ) {
                NavigationBar(
                    containerColor = MaterialTheme.colorScheme.surfaceContainer,
                    tonalElevation = 3.dp
                ) {
                    Screen.bottomNavItems.forEach { screen ->
                        val selected = currentRoute == screen.route
                        NavigationBarItem(
                            selected = selected,
                            onClick = {
                                navController.navigate(screen.route) {
                                    popUpTo(navController.graph.findStartDestination().id) {
                                        saveState = true
                                    }
                                    launchSingleTop = true
                                    restoreState = true
                                }
                            },
                            icon = {
                                Icon(
                                    imageVector = if (selected) screen.selectedIcon!! else screen.unselectedIcon!!,
                                    contentDescription = screen.label
                                )
                            },
                            label = { Text(screen.label) },
                            colors = NavigationBarItemDefaults.colors(
                                selectedIconColor = MaterialTheme.colorScheme.onSecondaryContainer,
                                selectedTextColor = MaterialTheme.colorScheme.onSecondaryContainer,
                                indicatorColor = MaterialTheme.colorScheme.secondaryContainer,
                                unselectedIconColor = MaterialTheme.colorScheme.onSurfaceVariant,
                                unselectedTextColor = MaterialTheme.colorScheme.onSurfaceVariant,
                            )
                        )
                    }
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = Screen.Stars.route,
            modifier = Modifier.padding(bottom = innerPadding.calculateBottomPadding())
        ) {
            composable(Screen.Stars.route) {
                StarsListScreen(
                    onRepoClick = { owner, repo ->
                        navController.navigate(Screen.StarDetail.createRoute(owner, repo))
                    },
                    onSearchClick = {
                        navController.navigate(Screen.Search.route)
                    },
                    onProfileClick = {
                        navController.navigate(Screen.Profile.route)
                    }
                )
            }
            composable(Screen.Explore.route) {
                ExploreScreen(
                    onRepoClick = { owner, repo ->
                        navController.navigate(Screen.StarDetail.createRoute(owner, repo))
                    },
                    onCategoryClick = { category ->
                        navController.navigate(Screen.CategoryResult.createRoute(category))
                    },
                    onProfileClick = {
                        navController.navigate(Screen.Profile.route)
                    }
                )
            }
            composable(Screen.Notifications.route) {
                NotificationsScreen(
                    onNotificationClick = { owner, repo ->
                        navController.navigate(Screen.StarDetail.createRoute(owner, repo))
                    },
                    onProfileClick = {
                        navController.navigate(Screen.Profile.route)
                    }
                )
            }
            composable(Screen.Profile.route) {
                ProfileScreen(onBack = { navController.popBackStack() })
            }
            composable(
                route = Screen.StarDetail.route,
                arguments = listOf(
                    navArgument("owner") { type = NavType.StringType },
                    navArgument("repo") { type = NavType.StringType }
                )
            ) { backStackEntry ->
                val owner = backStackEntry.arguments?.getString("owner") ?: ""
                val repo = backStackEntry.arguments?.getString("repo") ?: ""
                StarDetailScreen(
                    owner = owner,
                    repo = repo,
                    onBack = { navController.popBackStack() }
                )
            }
            composable(Screen.Search.route) {
                SearchScreen(onBack = { navController.popBackStack() })
            }
            composable(
                route = Screen.CategoryResult.route,
                arguments = listOf(
                    navArgument("category") { type = NavType.StringType }
                )
            ) { backStackEntry ->
                val category = backStackEntry.arguments?.getString("category") ?: ""
                CategoryResultScreen(
                    category = category,
                    onBack = { navController.popBackStack() },
                    onRepoClick = { owner: String, repo: String ->
                        navController.navigate(Screen.StarDetail.createRoute(owner, repo))
                    }
                )
            }
        }
    }
}
