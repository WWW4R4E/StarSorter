package com.www4r4e.compose.ui.explore

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.tooling.preview.Preview
import androidx.lifecycle.viewmodel.compose.viewModel
import com.www4r4e.compose.ui.UserViewModel
import com.www4r4e.compose.ui.components.UserAvatar

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ExploreScreen(
    onRepoClick: (String, String) -> Unit,
    onCategoryClick: (String) -> Unit,
    onProfileClick: () -> Unit = {},
    userViewModel: UserViewModel = viewModel()
) {
    val user by userViewModel.currentUser.collectAsState()

    Box(modifier = Modifier.fillMaxSize()) {
        TopAppBar(
            modifier = Modifier.statusBarsPadding(),
            title = { Text("Explore") },
            actions = {
                IconButton(onClick = onProfileClick) {
                    UserAvatar(
                        avatarUrl = user.avatarUrl,
                        contentDescription = "Profile",
                        size = 32.dp
                    )
                }
            },
            colors = TopAppBarDefaults.topAppBarColors(
                containerColor = MaterialTheme.colorScheme.surface
            )
        )
    }
}

@Preview
@Composable
fun ExploreScreenPreview() {
    ExploreScreen(
        onRepoClick = { _, _ -> },
        onCategoryClick = {}
    )
}