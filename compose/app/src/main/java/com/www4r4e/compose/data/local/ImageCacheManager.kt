package com.www4r4e.compose.data.local

import android.content.Context
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.encodeToString
import java.io.File
import java.io.FileOutputStream
import java.net.HttpURLConnection
import java.net.URL
import java.security.MessageDigest

/**
 * 图片缓存管理器
 * 负责管理图片的下载、缓存和过期清理
 */
class ImageCacheManager(
    private val context: Context,
    private val fileManager: LocalFileManager,
    private val cacheDurationDays: Long = 30,
    private val maxCacheSizeMB: Long = 100
) {
    
    /**
     * 获取缓存的图片文件（如果存在且未过期）
     */
    suspend fun getCachedImage(imageUrl: String): File? = withContext(Dispatchers.IO) {
        val hash = imageUrl.toMD5()
        val extensions = listOf(".jpg", ".png", ".webp")
        
        for (ext in extensions) {
            val cachedFile = File(fileManager.imagesCacheDir, "$hash$ext")
            if (cachedFile.exists()) {
                val cacheEntry = loadCacheIndex()[imageUrl]
                if (cacheEntry != null && !isExpired(cacheEntry.timestamp)) {
                    return@withContext cachedFile
                } else {
                    cachedFile.delete()
                    removeFromCacheIndex(imageUrl)
                }
            }
        }
        
        null
    }
    
    /**
     * 下载并缓存图片
     */
    suspend fun downloadAndCacheImage(imageUrl: String): File? = withContext(Dispatchers.IO) {
        try {
            val connection = URL(imageUrl).openConnection() as HttpURLConnection
            connection.connectTimeout = 10000
            connection.readTimeout = 10000
            connection.setRequestProperty("User-Agent", "Mozilla/5.0")
            
            if (connection.responseCode != 200) {
                return@withContext null
            }
            
            val inputStream = connection.inputStream
            val bitmap = BitmapFactory.decodeStream(inputStream)
            
            if (bitmap == null) {
                return@withContext null
            }
            
            val hash = imageUrl.toMD5()
            val extension = getImageExtension(imageUrl)
            val cachedFile = File(fileManager.imagesCacheDir, "$hash.$extension")
            
            FileOutputStream(cachedFile).use { fos ->
                when (extension) {
                    "png" -> bitmap.compress(Bitmap.CompressFormat.PNG, 100, fos)
                    "webp" -> bitmap.compress(Bitmap.CompressFormat.WEBP, 85, fos)
                    else -> bitmap.compress(Bitmap.CompressFormat.JPEG, 85, fos)
                }
            }
            
            addToCacheIndex(imageUrl, System.currentTimeMillis())
            
            checkAndCleanupCache()
            
            cachedFile
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
    
    /**
     * 获取或下载图片（优先使用缓存）
     */
    suspend fun getOrDownloadImage(imageUrl: String): File? {
        return getCachedImage(imageUrl) ?: downloadAndCacheImage(imageUrl)
    }
    
    /**
     * 手动清除所有图片缓存
     */
    suspend fun clearCache() = withContext(Dispatchers.IO) {
        fileManager.clearDirectory(fileManager.imagesCacheDir)
    }
    
    /**
     * 获取缓存大小（MB）
     */
    suspend fun getCacheSizeMB(): Double = withContext(Dispatchers.IO) {
        val bytes = fileManager.getDirectorySize(fileManager.imagesCacheDir)
        bytes.toDouble() / (1024 * 1024)
    }
    
    /**
     * 获取缓存图片数量
     */
    suspend fun getImageCount(): Int = withContext(Dispatchers.IO) {
        fileManager.imagesCacheDir.listFiles()?.count { it.isFile && it.name != "temp_index.json" } ?: 0
    }
    
    // ==================== 私有方法 ====================
    
    /**
     * 将字符串转换为 MD5 哈希值
     */
    private fun String.toMD5(): String {
        val bytes = MessageDigest.getInstance("MD5").digest(this.toByteArray())
        return bytes.joinToString("") { "%02x".format(it) }
    }
    
    /**
     * 判断是否过期
     */
    private fun isExpired(timestamp: Long): Boolean {
        val now = System.currentTimeMillis()
        val diffDays = (now - timestamp) / (1000 * 60 * 60 * 24)
        return diffDays >= cacheDurationDays
    }
    
    /**
     * 从 URL 推断图片扩展名
     */
    private fun getImageExtension(url: String): String {
        return when {
            url.endsWith(".png", ignoreCase = true) -> "png"
            url.endsWith(".webp", ignoreCase = true) -> "webp"
            url.contains("avatar", ignoreCase = true) -> "jpg"
            else -> "jpg"
        }
    }
    
    /**
     * 加载缓存索引
     */
    private fun loadCacheIndex(): Map<String, ImageCacheEntry> {
        return try {
            if (fileManager.imageCacheIndexFile.exists()) {
                val content = fileManager.imageCacheIndexFile.readText()
                fileManager.json.decodeFromString<Map<String, ImageCacheEntry>>(content)
            } else {
                emptyMap()
            }
        } catch (e: Exception) {
            emptyMap()
        }
    }
    
    /**
     * 保存缓存索引
     */
    private fun saveCacheIndex(index: Map<String, ImageCacheEntry>) {
        try {
            val content = fileManager.json.encodeToString(index)
            fileManager.imageCacheIndexFile.writeText(content)
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }
    
    /**
     * 添加缓存记录
     */
    private fun addToCacheIndex(url: String, timestamp: Long) {
        val index = loadCacheIndex().toMutableMap()
        index[url] = ImageCacheEntry(timestamp = timestamp)
        saveCacheIndex(index)
    }
    
    /**
     * 移除缓存记录
     */
    private fun removeFromCacheIndex(url: String) {
        val index = loadCacheIndex().toMutableMap()
        index.remove(url)
        saveCacheIndex(index)
    }
    
    /**
     * 检查并清理缓存（超出大小时删除最旧的 50%）
     */
    private suspend fun checkAndCleanupCache() = withContext(Dispatchers.IO) {
        val currentSizeMB = getCacheSizeMB()
        if (currentSizeMB > maxCacheSizeMB) {
            val index = loadCacheIndex()
            val sortedEntries = index.entries.sortedBy { it.value.timestamp }
            val entriesToRemove = sortedEntries.take(sortedEntries.size / 2)
            
            entriesToRemove.forEach { (url, _) ->
                val hash = url.toMD5()
                val extensions = listOf(".jpg", ".png", ".webp")
                for (ext in extensions) {
                    File(fileManager.imagesCacheDir, "$hash.$ext").delete()
                }
                removeFromCacheIndex(url)
            }
        }
    }
}

/**
 * 图片缓存条目数据类
 */
@kotlinx.serialization.Serializable
data class ImageCacheEntry(val timestamp: Long)
