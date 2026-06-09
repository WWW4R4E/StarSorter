package com.www4r4e.compose.ui.detail

import androidx.lifecycle.ViewModel
import com.www4r4e.compose.data.mock.MockRepos
import com.www4r4e.compose.data.model.Repository
import com.www4r4e.compose.data.mock.MockReleases
import com.www4r4e.compose.data.model.Release
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

data class DetailUiState(
    val repo: Repository? = null,
    val releases: List<Release> = emptyList(),
    val readmeContent: String = "",
    val selectedTab: Int = 0,
    val isStarred: Boolean = true
)

class StarDetailViewModel : ViewModel() {
    private val _uiState = MutableStateFlow(DetailUiState())
    val uiState: StateFlow<DetailUiState> = _uiState.asStateFlow()

    fun loadRepo(owner: String, repo: String) {
        val fullName = "$owner/$repo"
        val found = MockRepos.repos.find { it.fullName == fullName }
        val releases = MockReleases.releases[fullName] ?: emptyList()
        _uiState.value = DetailUiState(
            repo = found,
            releases = releases,
            readmeContent = "# $fullName\n\nThis is a mock README for $fullName.\n\n## Getting Started\n\n```kotlin\n// Add dependency\nimplementation(\"com.example:$repo:1.0.0\")\n```\n\n## Features\n\n- Feature 1\n- Feature 2\n- Feature 3\n\n## License\n\nMIT License\n\n## Contributing\n\nPull requests are welcome."
        )
    }

    fun onTabSelected(tab: Int) {
        _uiState.value = _uiState.value.copy(selectedTab = tab)
    }

    fun toggleStar() {
        _uiState.value = _uiState.value.copy(isStarred = !_uiState.value.isStarred)
    }
}
