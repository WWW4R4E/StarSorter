package com.www4r4e.compose.ui.stars

import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Search
import com.www4r4e.compose.ui.components.GitIcons
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.AssistChip
import androidx.compose.material3.AssistChipDefaults
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilterChip
import androidx.compose.material3.FilterChipDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.www4r4e.compose.ui.UserViewModel
import com.www4r4e.compose.ui.components.AdvancedFilterSheet
import com.www4r4e.compose.ui.components.EmptyState
import com.www4r4e.compose.ui.components.StarRepoCard
import com.www4r4e.compose.ui.components.UserAvatar
import com.www4r4e.compose.ui.theme.StarSorterTheme

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StarsListScreen(
    onRepoClick: (String, String) -> Unit,
    onSearchClick: () -> Unit,
    onProfileClick: () -> Unit = {},
    viewModel: StarsViewModel = viewModel(),
    userViewModel: UserViewModel = viewModel()
) {
    val state by viewModel.uiState.collectAsState()
    val user by userViewModel.currentUser.collectAsState()
    var showFilterSheet by remember { mutableStateOf(false) }
    var showAddListDialog by remember { mutableStateOf(false) }

    Column(
        modifier = Modifier.fillMaxSize()
    ) {
        TopAppBar(
            modifier = Modifier.statusBarsPadding(),
            title = {
                Text("All Stars")
            },
            actions = {
                IconButton(onClick = { showFilterSheet = true }) {
                    Icon(GitIcons.FilterList, contentDescription = "Filter")
                }
                IconButton(onClick = onSearchClick) {
                    Icon(Icons.Default.Search, contentDescription = "Search")
                }
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

        // Star List 筛选条 - 放在LazyColumn顶部
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            // Star List 筛选芯片
            if (state.starLists.isNotEmpty()) {
                item {
                    Row(
                        modifier = Modifier
                            .horizontalScroll(rememberScrollState())
                            .padding(vertical = 8.dp)
                            .pointerInput(Unit) {}, // 阻止手势传递到ModalBottomSheet
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        state.starLists.forEach { list ->
                            FilterChip(
                                selected = state.selectedListId == list.id,
                                onClick = {
                                    viewModel.setSelectedListId(list.id)
                                },
                                label = { Text(list.name) },
                                colors = FilterChipDefaults.filterChipColors(
                                    selectedContainerColor = MaterialTheme.colorScheme.secondaryContainer,
                                    selectedLabelColor = MaterialTheme.colorScheme.onSecondaryContainer
                                )
                            )
                        }
                        
                        // 添加新列表按钮
                        AssistChip(
                            onClick = { showAddListDialog = true },
                            label = { Text("Add List") },
                            leadingIcon = {
                                Icon(
                                    Icons.Default.Add,
                                    contentDescription = "Add"
                                )
                            },
                            colors = AssistChipDefaults.assistChipColors(
                                containerColor = MaterialTheme.colorScheme.primaryContainer,
                                labelColor = MaterialTheme.colorScheme.onPrimaryContainer
                            )
                        )
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                }
            }

            // 仓库列表
            if (state.filteredRepos.isEmpty()) {
                item {
                    EmptyState(
                        icon = Icons.Default.Star,
                        title = "No repositories found",
                        subtitle = if (state.searchQuery.isNotBlank()) "Try a different search" else "No repos match your filters"
                    )
                }
            } else {
                items(state.filteredRepos, key = { it.id }) { repo ->
                    StarRepoCard(
                        repo = repo,
                        onClick = { onRepoClick(repo.owner.login, repo.name) }
                    )
                }
            }
        }
    }

    if (showFilterSheet) {
        AdvancedFilterSheet(
            onDismiss = { showFilterSheet = false },
            viewModel = viewModel
        )
    }

    if (showAddListDialog) {
        AddListDialog(
            onDismiss = { showAddListDialog = false },
            onConfirm = { name, description ->
                viewModel.addNewStarList(name, description)
                showAddListDialog = false
            }
        )
    }
}

@Composable
fun AddListDialog(
    onDismiss: () -> Unit,
    onConfirm: (String, String?) -> Unit
) {
    var listName by remember { mutableStateOf("") }
    var listDescription by remember { mutableStateOf("") }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Create New List") },
        text = {
            Column {
                OutlinedTextField(
                    value = listName,
                    onValueChange = { listName = it },
                    label = { Text("List Name") },
                    placeholder = { Text("My Awesome List") },
                    modifier = Modifier.fillMaxWidth(),
                    singleLine = true,
                    keyboardOptions = KeyboardOptions(imeAction = ImeAction.Next),
                    keyboardActions = KeyboardActions(
                        onNext = { /* Focus moves to description */ }
                    )
                )
                Spacer(modifier = Modifier.height(8.dp))
                OutlinedTextField(
                    value = listDescription,
                    onValueChange = { listDescription = it },
                    label = { Text("Description (Optional)") },
                    placeholder = { Text("What's this list about?") },
                    modifier = Modifier.fillMaxWidth(),
                    maxLines = 3
                )
            }
        },
        confirmButton = {
            Button(
                onClick = { onConfirm(listName.trim(), listDescription.trim().takeIf { it.isNotEmpty() }) },
                enabled = listName.isNotBlank()
            ) {
                Text("Create")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    )
}
@Preview(showBackground = true)
@Composable
fun TrendingScreenPreview() {
    StarSorterTheme {
        StarsListScreen( onRepoClick = { _, _ -> }, onSearchClick = {  })
    }
}


