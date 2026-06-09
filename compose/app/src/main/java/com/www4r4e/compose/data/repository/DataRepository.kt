package com.www4r4e.compose.data.repository

import android.content.Context
import com.www4r4e.compose.data.local.ApiCacheManager
import com.www4r4e.compose.data.local.ImageCacheManager
import com.www4r4e.compose.data.local.LocalFileManager
import com.www4r4e.compose.data.mock.MockRepos
import com.www4r4e.compose.data.model.Notification
import com.www4r4e.compose.data.model.Repository
import com.www4r4e.compose.data.model.StarList
import com.www4r4e.compose.data.model.UserSettings
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.io.File

/**
 * 数据缓存状态
 */
data class CacheStats(
    val dataSizeMB: Double = 0.0,
    val imageCacheSizeMB: Double = 0.0,
    val apiCacheSizeMB: Double = 0.0,
    val totalCacheSizeMB: Double = 0.0,
    val imageCount: Int = 0
)

/**
 * 统一的数据仓库
 * 管理所有本地数据的读写操作
 */
class DataRepository(context: Context) {
    
    private val fileManager = LocalFileManager(context)
    private val imageCacheManager = ImageCacheManager(context, fileManager)
    private val apiCacheManager = ApiCacheManager(context, fileManager)
    
    // ==================== Stars 数据操作 ====================
    
    suspend fun saveStarsData(stars: List<StarList>, repos: List<Repository>) {
        fileManager.saveToFile(fileManager.starsFile, stars)
        fileManager.saveToFile(fileManager.starsFile.resolveSibling("repos.json"), repos)
    }
    
    suspend fun loadStarsData(): Pair<List<StarList>, List<Repository>> {
        val stars = fileManager.readFromFile<List<StarList>>(fileManager.starsFile) ?: emptyList()
        val reposFile = fileManager.starsFile.resolveSibling("repos.json")
        val repos = fileManager.readFromFile<List<Repository>>(reposFile) ?: emptyList()
        return Pair(stars, repos)
    }
    
    /**
     * 从 Mock 数据初始化（首次使用或刷新时）
     */
    suspend fun initializeFromMock() {
        val mockRepos = MockRepos.repos
        // TODO: 创建默认的 StarList
        val defaultList = StarList(
            id = "default",
            name = "All Stars",
            description = "All starred repositories",
            repos = mockRepos,
            createdAt = "2024-01-01T00:00:00Z",
            updatedAt = "2024-01-01T00:00:00Z",
            isPublic = true
        )
        saveStarsData(listOf(defaultList), mockRepos)
    }
    
    // ==================== Notifications 数据操作 ====================
    
    suspend fun saveNotifications(notifications: List<Notification>) {
        fileManager.saveToFile(fileManager.notificationsFile, notifications)
    }
    
    suspend fun loadNotifications(): List<Notification> {
        return fileManager.readFromFile<List<Notification>>(fileManager.notificationsFile) ?: emptyList()
    }
    
    // ==================== Categories 数据操作 ====================
    
    suspend fun saveCategories(categories: List<String>) {
        fileManager.saveToFile(fileManager.categoriesFile, categories)
    }
    
    suspend fun loadCategories(): List<String> {
        return fileManager.readFromFile<List<String>>(fileManager.categoriesFile) ?: emptyList()
    }
    
    // ==================== User Profile 数据操作 ====================
    
    suspend fun saveUserProfile(profile: Map<String, Any>) {
        fileManager.saveToFile(fileManager.userProfileFile, profile)
    }
    
    suspend fun loadUserProfile(): Map<String, Any>? {
        return fileManager.readFromFile<Map<String, Any>>(fileManager.userProfileFile)
    }
    
    // ==================== Settings 数据操作 ====================
    
    suspend fun saveSettings(settings: UserSettings) {
        fileManager.saveToFile(fileManager.settingsFile, settings)
    }
    
    suspend fun loadSettings(): UserSettings? {
        return fileManager.readFromFile<UserSettings>(fileManager.settingsFile)
    }
    
    // ==================== API 缓存操作 ====================
    
    suspend fun saveApiResponse(apiName: String, jsonData: String) {
        apiCacheManager.saveApiResponse(apiName, jsonData)
    }
    
    suspend fun getCachedApiResponse(apiName: String): String? {
        return apiCacheManager.getCachedApiResponse(apiName)
    }
    
    suspend fun isApiCacheValid(apiName: String): Boolean {
        return apiCacheManager.isCacheValid(apiName)
    }
    
    suspend fun clearApiCache(apiName: String) {
        apiCacheManager.clearApiCache(apiName)
    }
    
    suspend fun clearAllApiCache() {
        apiCacheManager.clearAllCache()
    }
    
    // ==================== 图片缓存操作 ====================
    
    suspend fun getCachedImage(imageUrl: String): java.io.File? {
        return imageCacheManager.getCachedImage(imageUrl)
    }
    
    suspend fun downloadAndCacheImage(imageUrl: String): java.io.File? {
        return imageCacheManager.downloadAndCacheImage(imageUrl)
    }
    
    suspend fun getOrDownloadImage(imageUrl: String): java.io.File? {
        return imageCacheManager.getOrDownloadImage(imageUrl)
    }
    
    suspend fun clearImageCache() {
        imageCacheManager.clearCache()
    }
    
    // ==================== 缓存统计 ====================
    
    suspend fun getCacheStats(): CacheStats {
        val dataSizeBytes = fileManager.getTotalDataSize()
        val imageCacheBytes = fileManager.getDirectorySize(fileManager.imagesCacheDir)
        val apiCacheBytes = fileManager.getDirectorySize(fileManager.apiCacheDir)
        val totalCacheBytes = imageCacheBytes + apiCacheBytes
        
        val imageCount = imageCacheManager.getImageCount()
        
        return CacheStats(
            dataSizeMB = dataSizeBytes.toDouble() / (1024 * 1024),
            imageCacheSizeMB = imageCacheBytes.toDouble() / (1024 * 1024),
            apiCacheSizeMB = apiCacheBytes.toDouble() / (1024 * 1024),
            totalCacheSizeMB = totalCacheBytes.toDouble() / (1024 * 1024),
            imageCount = imageCount
        )
    }
    
    // ==================== 清理操作 ====================
    
    /**
     * 清除所有缓存（保留用户数据）
     */
    suspend fun clearAllCache() {
        imageCacheManager.clearCache()
        apiCacheManager.clearAllCache()
    }
    
    /**
     * 清除所有数据（包括用户数据）
     */
    suspend fun clearAllData() {
        fileManager.clearDirectory(File(fileManager.context.filesDir, "data"))
        fileManager.clearDirectory(fileManager.context.cacheDir)
    }
    
    /**
     * 清理过期缓存
     */
    suspend fun cleanupExpiredCache() {
        apiCacheManager.cleanupExpiredCache()
    }
}
