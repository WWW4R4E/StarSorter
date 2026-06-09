package com.www4r4e.compose.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilterChip
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Slider
import androidx.compose.material3.Text
import androidx.compose.material3.rememberModalBottomSheetState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.www4r4e.compose.data.model.SortOption
import com.www4r4e.compose.data.model.SortOrder
import com.www4r4e.compose.ui.stars.StarsViewModel
import androidx.compose.foundation.layout.FlowRow

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AdvancedFilterSheet(
    onDismiss: () -> Unit,
    viewModel: StarsViewModel
) {
    val sheetState = rememberModalBottomSheetState()
    val state = viewModel.uiState.collectAsState()
    val filterOptions = state.value.filterOptions
    
    var selectedLanguages by remember { mutableStateOf(filterOptions.selectedLanguages) }
    var selectedTopics by remember { mutableStateOf(filterOptions.selectedTopics) }
    var sortBy by remember { mutableStateOf(filterOptions.sortBy) }
    var sortOrder by remember { mutableStateOf(filterOptions.sortOrder) }
    var showForks by remember { mutableStateOf(filterOptions.showForks) }
    var minStars by remember { mutableIntStateOf(filterOptions.minStars ?: 0) }
    var maxStars by remember { mutableIntStateOf(filterOptions.maxStars ?: 100000) }

    ModalBottomSheet(
        onDismissRequest = onDismiss,
        sheetState = sheetState
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 16.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = "Filters",
                    style = MaterialTheme.typography.titleLarge
                )
                IconButton(onClick = onDismiss) {
                    Icon(Icons.Default.Close, contentDescription = "Close")
                }
            }

            Spacer(modifier = Modifier.height(16.dp))

            LazyColumn(
                modifier = Modifier.weight(1f),
                verticalArrangement = Arrangement.spacedBy(24.dp)
            ) {
                // Language Filter
                if (filterOptions.languages.isNotEmpty()) {
                    item {
                        FilterSection(
                            title = "Language",
                            content = {
                                FlowRow(
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    filterOptions.languages.forEach { language ->
                                        FilterChip(
                                            selected = selectedLanguages.contains(language),
                                            onClick = {
                                                selectedLanguages = if (selectedLanguages.contains(language)) {
                                                    selectedLanguages - language
                                                } else {
                                                    selectedLanguages + language
                                                }
                                            },
                                            label = { Text(language) }
                                        )
                                    }
                                }
                            }
                        )
                    }
                }

                // Topics Filter
                if (filterOptions.topics.isNotEmpty()) {
                    item {
                        FilterSection(
                            title = "Topics",
                            content = {
                                FlowRow(
                                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                                    verticalArrangement = Arrangement.spacedBy(8.dp)
                                ) {
                                    filterOptions.topics.forEach { topic ->
                                        FilterChip(
                                            selected = selectedTopics.contains(topic),
                                            onClick = {
                                                selectedTopics = if (selectedTopics.contains(topic)) {
                                                    selectedTopics - topic
                                                } else {
                                                    selectedTopics + topic
                                                }
                                            },
                                            label = { Text(topic) }
                                        )
                                    }
                                }
                            }
                        )
                    }
                }

                // Sort Options
                item {
                    FilterSection(
                        title = "Sort By",
                        content = {
                            FlowRow(
                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                verticalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                SortOption.entries.forEach { option ->
                                    FilterChip(
                                        selected = sortBy == option,
                                        onClick = { sortBy = option },
                                        label = { Text(option.label) }
                                    )
                                }
                            }
                        }
                    )
                }

                // Sort Order
                item {
                    FilterSection(
                        title = "Sort Order",
                        content = {
                            FlowRow(
                                horizontalArrangement = Arrangement.spacedBy(8.dp),
                                verticalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                SortOrder.entries.forEach { order ->
                                    FilterChip(
                                        selected = sortOrder == order,
                                        onClick = { sortOrder = order },
                                        label = { Text(order.label) }
                                    )
                                }
                            }
                        }
                    )
                }

                // Stars Range
                item {
                    FilterSection(
                        title = "Stars Range",
                        content = {
                            Column {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    Text(
                                        text = if (minStars >= 100000) "100k+" else "$minStars",
                                        style = MaterialTheme.typography.bodyMedium
                                    )
                                    Text(
                                        text = if (maxStars >= 100000) "100k+" else "$maxStars",
                                        style = MaterialTheme.typography.bodyMedium
                                    )
                                }
                                Spacer(modifier = Modifier.height(8.dp))
                                Slider(
                                    value = minStars.toFloat(),
                                    onValueChange = { minStars = it.toInt() },
                                    valueRange = 0f..100000f,
                                    steps = 19
                                )
                                Slider(
                                    value = maxStars.toFloat(),
                                    onValueChange = { maxStars = it.toInt() },
                                    valueRange = 0f..100000f,
                                    steps = 19
                                )
                            }
                        }
                    )
                }

                // Show Forks
                item {
                    FilterSection(
                        title = "Show Forks",
                        content = {
                            FilterChip(
                                selected = showForks,
                                onClick = { showForks = !showForks },
                                label = { Text(if (showForks) "Yes" else "No") }
                            )
                        }
                    )
                }
            }

            Spacer(modifier = Modifier.height(16.dp))

            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 16.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                OutlinedButton(
                    onClick = {
                        viewModel.resetFilters()
                        onDismiss()
                    },
                    modifier = Modifier.weight(1f)
                ) {
                    Text("Reset")
                }
                Button(
                    onClick = {
                        viewModel.updateFilterOptions {
                            copy(
                                selectedLanguages = selectedLanguages,
                                selectedTopics = selectedTopics,
                                sortBy = sortBy,
                                sortOrder = sortOrder,
                                showForks = showForks,
                                minStars = if (minStars == 0) null else minStars,
                                maxStars = if (maxStars >= 100000) null else maxStars
                            )
                        }
                        onDismiss()
                    },
                    modifier = Modifier.weight(1f)
                ) {
                    Text("Apply Filters")
                }
            }
        }
    }
}

@Composable
fun FilterSection(
    title: String,
    content: @Composable () -> Unit
) {
    Column {
        Text(
            text = title,
            style = MaterialTheme.typography.titleSmall,
            color = MaterialTheme.colorScheme.primary
        )
        Spacer(modifier = Modifier.height(8.dp))
        content()
    }
}

