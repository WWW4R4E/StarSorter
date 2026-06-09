package com.www4r4e.compose.ui.discover

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import com.www4r4e.compose.ui.components.GitIcons
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.www4r4e.compose.ui.theme.StarSorterTheme
import com.www4r4e.compose.ui.components.EmptyState
import com.www4r4e.compose.ui.components.StarRepoCard

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CategoryResultScreen(
    category: String,
    onBack: () -> Unit,
    onRepoClick: (String, String) -> Unit,
    viewModel: DiscoverViewModel = viewModel()
) {
    val state by viewModel.categoryState.collectAsState()

    LaunchedEffect(category) {
        viewModel.loadCategory(category)
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(
                        text = category.replace("_", " ")
                    )
                },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface
                )
            )
        }
    ) { padding ->
        if (state.repos.isEmpty()) {
            EmptyState(
                icon = GitIcons.Whatshot,
                title = "No repositories found",
                subtitle = "Check back later for more results"
            )
        } else {
            LazyColumn(
                contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
            ) {
                items(state.repos, key = { it.id }) { repo ->
                    StarRepoCard(
                        repo = repo,
                        onClick = { onRepoClick(repo.owner.login, repo.name) }
                    )
                }
            }
        }
    }
}

@Preview(showBackground = true, showSystemUi = true)
@Composable
fun CategoryResultScreenPreview() {
    StarSorterTheme {
        CategoryResultScreen(
            category = "Trending",
            onBack = {},
            onRepoClick = { _, _ -> }
        )
    }
}
