package com.www4r4e.compose.ui.discover

import androidx.lifecycle.ViewModel
import com.www4r4e.compose.data.model.DiscoveryChannel
import com.www4r4e.compose.data.model.Repository
import com.www4r4e.compose.data.mock.MockRepos
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

data class DiscoverUiState(
    val channels: List<DiscoveryChannel> = DiscoveryChannel.entries.filter { it != DiscoveryChannel.DEVELOPERS }
)

data class CategoryResultUiState(
    val category: String = "",
    val repos: List<Repository> = emptyList()
)

class DiscoverViewModel : ViewModel() {
    private val _uiState = MutableStateFlow(DiscoverUiState())
    val uiState: StateFlow<DiscoverUiState> = _uiState.asStateFlow()

    private val _categoryState = MutableStateFlow(CategoryResultUiState())
    val categoryState: StateFlow<CategoryResultUiState> = _categoryState.asStateFlow()

    fun loadCategory(category: String) {
        val repos = MockRepos.repos.shuffled().take(10)
        _categoryState.value = CategoryResultUiState(category = category, repos = repos)
    }
}
