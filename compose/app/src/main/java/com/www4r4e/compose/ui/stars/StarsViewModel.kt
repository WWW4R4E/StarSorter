package com.www4r4e.compose.ui.stars

import androidx.lifecycle.ViewModel
import com.www4r4e.compose.data.mock.MockRepos
import com.www4r4e.compose.data.model.FilterOptions
import com.www4r4e.compose.data.model.Repository
import com.www4r4e.compose.data.model.SortOption
import com.www4r4e.compose.data.model.SortOrder
import com.www4r4e.compose.data.model.StarList
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.time.LocalDate
import java.time.format.DateTimeFormatter

data class StarsUiState(
    val starLists: List<StarList> = emptyList(),
    val allRepos: List<Repository> = emptyList(),
    val filteredRepos: List<Repository> = emptyList(),
    val selectedListId: String? = null, // null means all repos
    val filterOptions: FilterOptions = FilterOptions(),
    val searchQuery: String = ""
)

class StarsViewModel : ViewModel() {
    private val _uiState = MutableStateFlow(StarsUiState())
    val uiState: StateFlow<StarsUiState> = _uiState.asStateFlow()

    init {
        val repos = MockRepos.repos
        val languages = repos.mapNotNull { it.language }.distinct().sorted()
        val topics = repos.flatMap { it.topics ?: emptyList() }.distinct().sorted()
        
        // 创建默认的 "All Stars" 列表
        val allStarsList = StarList(
            id = "all_stars",
            name = "All Stars",
            description = "All starred repositories",
            repos = repos,
            createdAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE),
            updatedAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE)
        )
        
        // 创建一些示例列表
        val sampleLists = listOf(
            StarList(
                id = "kotlin_repos",
                name = "Kotlin Projects",
                description = "Repositories using Kotlin",
                repos = repos.filter { it.language == "Kotlin" },
                createdAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE),
                updatedAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE)
            ),
            StarList(
                id = "compose_repos",
                name = "Jetpack Compose",
                description = "Compose related repositories",
                repos = repos.filter { it.topics?.contains("compose") == true },
                createdAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE),
                updatedAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE)
            ),
            StarList(
                id = "android_repos",
                name = "Android Development",
                description = "Android related repositories",
                repos = repos.filter { it.topics?.contains("android") == true },
                createdAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE),
                updatedAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE)
            )
        )
        
        _uiState.value = StarsUiState(
            starLists = listOf(allStarsList) + sampleLists,
            allRepos = repos,
            filteredRepos = repos,
            selectedListId = "all_stars", // 默认选中All Stars
            filterOptions = FilterOptions(
                languages = languages,
                topics = topics
            )
        )
    }

    fun setSelectedListId(listId: String?) {
        _uiState.value = _uiState.value.copy(
            selectedListId = listId
        )
        applyFilters()
    }

    fun addNewStarList(name: String, description: String? = null) {
        val currentState = _uiState.value
        val newList = StarList(
            id = "list_${System.currentTimeMillis()}",
            name = name,
            description = description,
            repos = emptyList(),
            createdAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE),
            updatedAt = LocalDate.now().format(DateTimeFormatter.ISO_DATE)
        )
        _uiState.value = currentState.copy(
            starLists = currentState.starLists + newList
        )
    }

    fun updateFilterOptions(update: FilterOptions.() -> FilterOptions) {
        val currentOptions = _uiState.value.filterOptions
        _uiState.value = _uiState.value.copy(
            filterOptions = currentOptions.update()
        )
        applyFilters()
    }

    fun resetFilters() {
        val currentOptions = _uiState.value.filterOptions
        _uiState.value = _uiState.value.copy(
            filterOptions = currentOptions.copy(
                selectedLanguages = emptySet(),
                selectedTopics = emptySet(),
                sortBy = SortOption.STARGAZERS,
                sortOrder = SortOrder.DESCENDING,
                showForks = true,
                minStars = null,
                maxStars = null
            )
        )
        applyFilters()
    }

    private fun applyFilters() {
        val state = _uiState.value
        
        // 根据选中的list获取repos
        var repos = if (state.selectedListId == null || state.selectedListId == "all_stars") {
            state.allRepos
        } else {
            state.starLists.find { it.id == state.selectedListId }?.repos ?: state.allRepos
        }
        
        val filtered = applyFiltersToList(repos)
        _uiState.value = state.copy(filteredRepos = filtered)
    }

    private fun applyFiltersToList(repos: List<Repository>): List<Repository> {
        val state = _uiState.value
        val options = state.filterOptions
        var filtered = repos

        // 语言筛选
        if (options.selectedLanguages.isNotEmpty()) {
            filtered = filtered.filter { 
                it.language != null && options.selectedLanguages.contains(it.language) 
            }
        }

        // 主题筛选
        if (options.selectedTopics.isNotEmpty()) {
            filtered = filtered.filter { repo ->
                repo.topics?.any { options.selectedTopics.contains(it) } == true
            }
        }

        // 搜索查询
        if (state.searchQuery.isNotBlank()) {
            filtered = filtered.filter {
                it.fullName.contains(state.searchQuery, ignoreCase = true) ||
                (it.description?.contains(state.searchQuery, ignoreCase = true) == true)
            }
        }

        // 显示/隐藏 Forks
        if (!options.showForks) {
            filtered = filtered.filter { !it.fork }
        }

        // 星级范围筛选
        if (options.minStars != null) {
            filtered = filtered.filter { it.stargazersCount >= options.minStars }
        }
        if (options.maxStars != null) {
            filtered = filtered.filter { it.stargazersCount <= options.maxStars }
        }

        // 排序
        filtered = when (options.sortBy) {
            SortOption.STARGAZERS -> filtered.sortedByDescending { it.stargazersCount }
            SortOption.FORKS -> filtered.sortedByDescending { it.forksCount }
            SortOption.NAME -> filtered.sortedBy { it.fullName.lowercase() }
            SortOption.UPDATED -> filtered.sortedByDescending { it.updatedAt }
            SortOption.CREATED -> filtered.sortedByDescending { it.createdAt }
        }

        // 排序顺序
        if (options.sortOrder == SortOrder.ASCENDING && options.sortBy != SortOption.NAME) {
            filtered = filtered.reversed()
        }

        return filtered
    }
}
