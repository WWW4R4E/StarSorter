package com.www4r4e.compose.ui.search

import androidx.lifecycle.ViewModel
import com.www4r4e.compose.data.mock.MockRepos
import com.www4r4e.compose.data.model.Repository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

data class SearchUiState(
    val query: String = "",
    val results: List<Repository> = emptyList(),
    val isSearching: Boolean = false,
    val recentSearches: List<String> = listOf("compose", "kotlin multiplatform", "jetpack", "material3")
)

class SearchViewModel : ViewModel() {
    private val _uiState = MutableStateFlow(SearchUiState())
    val uiState: StateFlow<SearchUiState> = _uiState.asStateFlow()

    fun onQueryChanged(query: String) {
        _uiState.value = _uiState.value.copy(query = query)
        if (query.length >= 2) {
            val results = MockRepos.repos.filter {
                it.fullName.contains(query, ignoreCase = true) ||
                (it.description?.contains(query, ignoreCase = true) == true)
            }
            _uiState.value = _uiState.value.copy(results = results, isSearching = false)
        } else {
            _uiState.value = _uiState.value.copy(results = emptyList(), isSearching = false)
        }
    }

    fun onSearch(query: String) {
        _uiState.value = _uiState.value.copy(isSearching = true)
        onQueryChanged(query)
    }
}
