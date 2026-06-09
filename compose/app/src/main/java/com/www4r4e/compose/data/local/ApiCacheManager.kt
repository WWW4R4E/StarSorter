package com.www4r4e.compose.data.local

import android.content.Context
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.encodeToString
import java.io.File

/**
 * API 响应缓存管理器
 * 负责缓存 API 返回的 JSON 数据，避免重复网络请求
 */
class ApiCacheManager(
    private val context: Context,
    private val fileManager: LocalFileManager,
    private val cacheDurationHours: Long = 24
) {
    
    /**
     * 保存 API 响应到缓存
     */
    suspend fun saveApiResponse(apiName: String, jsonData: String) = withContext(Dispatchers.IO) {
        try {
            val cacheFile = File(fileManager.apiCacheDir, "${apiName}.json")
            val metaFile = File(fileManager.apiCacheDir, "${apiName}_meta.json")
            
            cacheFile.writeText(jsonData)
            
            val meta = ApiCacheMeta(
                timestamp = System.currentTimeMillis(),
                apiName = apiName
            )
            val metaJson = fileManager.json.encodeToString(meta)
            metaFile.writeText(metaJson)
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }
    
    /**
     * 获取缓存的 API 响应（如果存在且未过期）
     */
    suspend fun getCachedApiResponse(apiName: String): String? = withContext(Dispatchers.IO) {
        try {
            val cacheFile = File(fileManager.apiCacheDir, "${apiName}.json")
            val metaFile = File(fileManager.apiCacheDir, "${apiName}_meta.json")
            
            if (!cacheFile.exists() || !metaFile.exists()) {
                return@withContext null
            }
            
            val metaJson = metaFile.readText()
            val meta = fileManager.json.decodeFromString<ApiCacheMeta>(metaJson)
            
            if (isExpired(meta.timestamp)) {
                cacheFile.delete()
                metaFile.delete()
                return@withContext null
            }
            
            cacheFile.readText()
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
    
    /**
     * 检查 API 缓存是否存在且有效
     */
    suspend fun isCacheValid(apiName: String): Boolean = withContext(Dispatchers.IO) {
        val metaFile = File(fileManager.apiCacheDir, "${apiName}_meta.json")
        if (!metaFile.exists()) return@withContext false
        
        try {
            val metaJson = metaFile.readText()
            val meta = fileManager.json.decodeFromString<ApiCacheMeta>(metaJson)
            !isExpired(meta.timestamp)
        } catch (e: Exception) {
            false
        }
    }
    
    /**
     * 清除特定 API 的缓存
     */
    suspend fun clearApiCache(apiName: String) = withContext(Dispatchers.IO) {
        val cacheFile = File(fileManager.apiCacheDir, "${apiName}.json")
        val metaFile = File(fileManager.apiCacheDir, "${apiName}_meta.json")
        
        cacheFile.delete()
        metaFile.delete()
    }
    
    /**
     * 清除所有 API 缓存
     */
    suspend fun clearAllCache() = withContext(Dispatchers.IO) {
        fileManager.clearDirectory(fileManager.apiCacheDir)
    }
    
    /**
     * 获取 API 缓存大小（MB）
     */
    suspend fun getCacheSizeMB(): Double = withContext(Dispatchers.IO) {
        val bytes = fileManager.getDirectorySize(fileManager.apiCacheDir)
        bytes.toDouble() / (1024 * 1024)
    }
    
    /**
     * 清理过期的缓存
     */
    suspend fun cleanupExpiredCache() = withContext(Dispatchers.IO) {
        val files = fileManager.apiCacheDir.listFiles()?.filter { it.name.endsWith("_meta.json") } ?: return@withContext
        
        files.forEach { metaFile ->
            try {
                val metaJson = metaFile.readText()
                val meta = fileManager.json.decodeFromString<ApiCacheMeta>(metaJson)
                
                if (isExpired(meta.timestamp)) {
                    val apiName = meta.apiName
                    clearApiCache(apiName)
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }
    
    // ==================== 私有方法 ====================
    
    private fun isExpired(timestamp: Long): Boolean {
        val now = System.currentTimeMillis()
        val diffHours = (now - timestamp) / (1000 * 60 * 60)
        return diffHours >= cacheDurationHours
    }
}

/**
 * API 缓存元数据
 */
@kotlinx.serialization.Serializable
data class ApiCacheMeta(
    val timestamp: Long,
    val apiName: String
)
