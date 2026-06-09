package com.www4r4e.compose.ui.detail

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Share
import androidx.compose.material.icons.filled.Star
import com.www4r4e.compose.ui.components.GitIcons
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilledTonalButton
import androidx.compose.material3.FilterChip
import androidx.compose.material3.FilterChipDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Tab
import androidx.compose.material3.PrimaryTabRow
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.www4r4e.compose.ui.theme.StarSorterTheme
import com.www4r4e.compose.ui.components.ReleaseCard
import com.www4r4e.compose.ui.components.RepoStatRow

import com.www4r4e.compose.ui.components.UserAvatar
import com.www4r4e.compose.ui.components.formatCount

@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun StarDetailScreen(
    owner: String,
    repo: String,
    onBack: () -> Unit,
    viewModel: StarDetailViewModel = viewModel()
) {
    val state by viewModel.uiState.collectAsState()

    LaunchedEffect(owner, repo) {
        viewModel.loadRepo(owner, repo)
    }

    Column(
        modifier = Modifier.fillMaxSize()
    ) {
        TopAppBar(
            modifier = Modifier.statusBarsPadding(),
            title = { Text("$owner/$repo") },
            navigationIcon = {
                IconButton(onClick = onBack) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                }
            },
            actions = {
                IconButton(onClick = { }) {
                    Icon(Icons.Default.Share, contentDescription = "Share")
                }
                IconButton(onClick = { }) {
                    Icon(GitIcons.OpenInBrowser, contentDescription = "Open in Browser")
                }
            },
            colors = TopAppBarDefaults.topAppBarColors(
                containerColor = MaterialTheme.colorScheme.surface
            )
        )

        if (state.repo == null) {
            // Loading / not found state
            return
        }

        Column(
            modifier = Modifier.fillMaxSize()
        ) {
            // Header
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 8.dp)
            ) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    UserAvatar(
                        avatarUrl = state.repo!!.owner.avatarUrl,
                        contentDescription = state.repo!!.owner.login,
                        size = 48.dp
                    )
                    Spacer(modifier = Modifier.width(12.dp))
                    Column {
                        Text(
                            text = state.repo!!.fullName,
                            style = MaterialTheme.typography.titleLarge
                        )
                        Text(
                            text = state.repo!!.owner.login,
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }

                if (!state.repo!!.description.isNullOrEmpty()) {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = state.repo!!.description!!,
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }

                Spacer(modifier = Modifier.height(12.dp))
                RepoStatRow(
                    stars = state.repo!!.stargazersCount,
                    forks = state.repo!!.forksCount,
                    language = state.repo!!.language
                )

                if (!state.repo!!.topics.isNullOrEmpty()) {
                    Spacer(modifier = Modifier.height(8.dp))
                    FlowRow(
                        horizontalArrangement = Arrangement.spacedBy(6.dp),
                        verticalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        state.repo!!.topics!!.take(8).forEach { topic ->
                            FilterChip(
                                selected = false,
                                onClick = { },
                                label = {
                                    Text(topic, style = MaterialTheme.typography.labelSmall)
                                },
                                colors = FilterChipDefaults.filterChipColors(
                                    containerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                                    labelColor = MaterialTheme.colorScheme.onSurfaceVariant
                                ),
                                border = null
                            )
                        }
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))
                // Action buttons
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Button(
                        onClick = { viewModel.toggleStar() },
                        colors = ButtonDefaults.buttonColors(
                            containerColor = if (state.isStarred)
                                MaterialTheme.colorScheme.primary
                            else
                                MaterialTheme.colorScheme.surfaceContainerHigh
                        )
                    ) {
                        Icon(
                            Icons.Default.Star,
                            contentDescription = null,
                            tint = if (state.isStarred) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.tertiary,
                            modifier = Modifier.size(18.dp)
                        )
                        Spacer(modifier = Modifier.width(6.dp))
                        Text(if (state.isStarred) "Starred (${formatCount(state.repo!!.stargazersCount)})" else "Unstar")
                    }
                    FilledTonalButton(onClick = { }) {
                        Icon(GitIcons.Link, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(6.dp))
                        Text("Clone")
                    }
                    OutlinedButton(onClick = { }) {
                        Icon(GitIcons.OpenInBrowser, contentDescription = null, modifier = Modifier.size(18.dp))
                    }
                }
            }

            Spacer(modifier = Modifier.height(8.dp))

            // Tabs
            PrimaryTabRow(
                selectedTabIndex = state.selectedTab,
                containerColor = MaterialTheme.colorScheme.surface,
                contentColor = MaterialTheme.colorScheme.primary
            ) {
                Tab(selected = state.selectedTab == 0, onClick = { viewModel.onTabSelected(0) }) {
                    Text("README", modifier = Modifier.padding(16.dp))
                }
                Tab(selected = state.selectedTab == 1, onClick = { viewModel.onTabSelected(1) }) {
                    Text("Releases (${state.releases.size})", modifier = Modifier.padding(16.dp))
                }
                Tab(selected = state.selectedTab == 2, onClick = { viewModel.onTabSelected(2) }) {
                    Text("Info", modifier = Modifier.padding(16.dp))
                }
            }

            when (state.selectedTab) {
                0 -> ReadmeTab(readmeContent = state.readmeContent)
                1 -> ReleasesTab(releases = state.releases)
                2 -> InfoTab(repo = state.repo!!)
            }
        }
    }
}

@Composable
private fun ReadmeTab(readmeContent: String) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(16.dp)
    ) {
        val lines = readmeContent.split("\n")
        var inCodeBlock = false
        lines.forEach { line ->
            when {
                line.startsWith("``") -> {
                    inCodeBlock = !inCodeBlock
                }
                inCodeBlock -> {
                    Text(
                        text = line,
                        style = MaterialTheme.typography.bodySmall,
                        fontFamily = androidx.compose.ui.text.font.FontFamily.Monospace,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        modifier = Modifier.padding(start = 8.dp)
                    )
                }
                line.startsWith("## ") -> {
                    Spacer(modifier = Modifier.height(12.dp))
                    Text(
                        text = line.removePrefix("## "),
                        style = MaterialTheme.typography.titleMedium
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                }
                line.startsWith("# ") -> {
                    Text(
                        text = line.removePrefix("# "),
                        style = MaterialTheme.typography.headlineSmall
                    )
                    Spacer(modifier = Modifier.height(8.dp))
                }
                line.startsWith("- ") -> {
                    Row {
                        Text("  •  ", color = MaterialTheme.colorScheme.onSurfaceVariant)
                        Text(
                            text = line.removePrefix("- "),
                            style = MaterialTheme.typography.bodyMedium
                        )
                    }
                }
                line.isBlank() -> Spacer(modifier = Modifier.height(4.dp))
                else -> {
                    Text(
                        text = line,
                        style = MaterialTheme.typography.bodyMedium
                    )
                }
            }
        }
    }
}

@Composable
private fun ReleasesTab(releases: List<com.www4r4e.compose.data.model.Release>) {
    if (releases.isEmpty()) {
        Text(
            text = "No releases yet",
            modifier = Modifier.padding(32.dp),
            style = MaterialTheme.typography.bodyLarge,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        return
    }
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = androidx.compose.foundation.layout.PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        items(releases, key = { it.id }) { release ->
            ReleaseCard(release = release)
        }
    }
}

@Composable
private fun InfoTab(repo: com.www4r4e.compose.data.model.Repository) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(16.dp)
    ) {
        InfoRow("License", repo.license?.name ?: "Not specified")
        InfoRow("Homepage", repo.homepage ?: "Not specified")
        InfoRow("Default branch", repo.defaultBranch)
        InfoRow("Visibility", repo.visibility ?: "public")
        InfoRow("Created", repo.createdAt.take(10))
        InfoRow("Updated", repo.updatedAt.take(10))
        if (repo.fork) {
            Spacer(modifier = Modifier.height(12.dp))
            Text(
                "This is a fork",
                style = MaterialTheme.typography.titleSmall,
                color = MaterialTheme.colorScheme.tertiary
            )
        }
    }
}

@Composable
private fun InfoRow(label: String, value: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 6.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            modifier = Modifier.weight(0.35f)
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodyMedium,
            modifier = Modifier.weight(0.65f)
        )
    }
}

@Preview(showBackground = true, showSystemUi = true)
@Composable
fun StarDetailScreenPreview() {
    StarSorterTheme {
        StarDetailScreen(
            owner = "octocat",
            repo = "Hello-World",
            onBack = {}
        )
    }
}
