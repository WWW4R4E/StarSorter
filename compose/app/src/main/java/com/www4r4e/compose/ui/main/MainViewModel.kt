package com.www4r4e.compose.ui.main

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.www4r4e.compose.data.repository.CacheStats
import com.www4r4e.compose.data.repository.DataRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * 主应用 ViewModel
 * 管理全局数据状态和缓存统计
 */
data class MainUiState(
    val isLoading: Boolean = false,
    val cacheStats: CacheStats = CacheStats(),
    val lastSyncTime: Long? = null,
    val errorMessage: String? = null
)

class MainViewModel(application: Application) : AndroidViewModel(application) {
    
    private val repository = DataRepository(application)
    
    private val _uiState = MutableStateFlow(MainUiState())
    val uiState: StateFlow<MainUiState> = _uiState.asStateFlow()
    
    init {
        loadInitialData()
        updateCacheStats()
    }
    
    /**
     * 加载初始数据（如果不存在则从 Mock 初始化）
     */
    private fun loadInitialData() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            
            try {
                val (stars, repos) = repository.loadStarsData()
                
                // 如果没有数据，从 Mock 初始化
                if (stars.isEmpty() && repos.isEmpty()) {
                    repository.initializeFromMock()
                }
                
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    lastSyncTime = System.currentTimeMillis()
                )
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    errorMessage = e.message
                )
            }
        }
    }
    
    /**
     * 刷新所有数据
     */
    fun refreshData() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            
            try {
                repository.initializeFromMock()
                updateCacheStats()
                
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    lastSyncTime = System.currentTimeMillis()
                )
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    errorMessage = e.message
                )
            }
        }
    }
    
    /**
     * 清除所有缓存（保留用户数据）
     */
    fun clearCache() {
        viewModelScope.launch {
            repository.clearAllCache()
            updateCacheStats()
        }
    }
    
    /**
     * 清除图片缓存
     */
    fun clearImageCache() {
        viewModelScope.launch {
            repository.clearImageCache()
            updateCacheStats()
        }
    }
    
    /**
     * 清除 API 缓存
     */
    fun clearApiCache() {
        viewModelScope.launch {
            repository.clearAllApiCache()
            updateCacheStats()
        }
    }
    
    /**
     * 清除所有数据（包括用户数据）
     */
    fun clearAllData() {
        viewModelScope.launch {
            repository.clearAllData()
            updateCacheStats()
            // 重新初始化
            loadInitialData()
        }
    }
    
    /**
     * 更新缓存统计信息
     */
    private fun updateCacheStats() {
        viewModelScope.launch {
            val stats = repository.getCacheStats()
            _uiState.value = _uiState.value.copy(cacheStats = stats)
        }
    }
    
    /**
     * 清理过期缓存
     */
    fun cleanupExpiredCache() {
        viewModelScope.launch {
            repository.cleanupExpiredCache()
            updateCacheStats()
        }
    }
}
