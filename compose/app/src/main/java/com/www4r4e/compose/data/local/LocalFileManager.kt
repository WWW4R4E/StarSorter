package com.www4r4e.compose.data.local

import android.content.Context
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import java.io.File

/**
 * 本地文件管理器
 * 负责管理应用的文件存储结构
 */
class LocalFileManager(val context: Context) {
    
    @PublishedApi
    internal val json = Json {
        prettyPrint = true
        ignoreUnknownKeys = true
        encodeDefaults = true
    }
    
    // ==================== 目录结构 ====================
    
    /**
     * 数据根目录: /data/data/com.www4r4e.compose/files/
     */
    private val dataRootDir: File by lazy {
        File(context.filesDir, "data").apply {
            if (!exists()) mkdirs()
        }
    }
    
    /**
     * 缓存根目录: /data/data/com.www4r4e.compose/cache/
     */
    private val cacheRootDir: File by lazy {
        context.cacheDir.apply {
            if (!exists()) mkdirs()
        }
    }
    
    /**
     * 图片缓存目录
     */
    val imagesCacheDir: File by lazy {
        File(cacheRootDir, "images").apply {
            if (!exists()) mkdirs()
        }
    }
    
    /**
     * API 响应缓存目录
     */
    val apiCacheDir: File by lazy {
        File(cacheRootDir, "api").apply {
            if (!exists()) mkdirs()
        }
    }
    
    /**
     * 配置目录
     */
    val configDir: File by lazy {
        File(dataRootDir, "config").apply {
            if (!exists()) mkdirs()
        }
    }
    
    // ==================== 数据文件路径 ====================
    
    val starsFile: File get() = File(dataRootDir, "stars.json")
    val notificationsFile: File get() = File(dataRootDir, "notifications.json")
    val categoriesFile: File get() = File(dataRootDir, "categories.json")
    val userProfileFile: File get() = File(dataRootDir, "user_profile.json")
    
    // ==================== 缓存文件路径 ====================
    
    val imageCacheIndexFile: File get() = File(imagesCacheDir, "temp_index.json")
    
    // ==================== 配置文件路径 ====================
    
    val settingsFile: File get() = File(configDir, "settings.json")
    
    // ==================== 通用文件操作 ====================
    
    /**
     * 保存 JSON 数据到文件
     */
    suspend inline fun <reified T> saveToFile(file: File, data: T) = withContext(Dispatchers.IO) {
        try {
            val jsonString = json.encodeToString(data)
            file.writeText(jsonString)
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }
    
    /**
     * 从文件读取 JSON 数据
     */
    @PublishedApi
    internal suspend inline fun <reified T> readFromFileInternal(file: File): T? = withContext(Dispatchers.IO) {
        try {
            if (!file.exists()) return@withContext null
            val jsonString = file.readText()
            if (jsonString.isBlank()) return@withContext null
            json.decodeFromString<T>(jsonString)
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
    
    /**
     * 从文件读取 JSON 数据（公开接口）
     */
    suspend inline fun <reified T> readFromFile(file: File): T? {
        return readFromFileInternal<T>(file)
    }
    
    /**
     * 检查文件是否存在
     */
    fun fileExists(file: File): Boolean {
        return file.exists()
    }
    
    /**
     * 删除文件
     */
    suspend fun deleteFile(file: File) = withContext(Dispatchers.IO) {
        if (file.exists()) {
            file.delete()
        }
    }
    
    /**
     * 获取文件大小（字节）
     */
    fun getFileSize(file: File): Long {
        return if (file.exists()) file.length() else 0L
    }
    
    /**
     * 获取目录总大小（字节）
     */
    suspend fun getDirectorySize(directory: File): Long = withContext(Dispatchers.IO) {
        if (!directory.exists()) return@withContext 0L
        
        directory.walkTopDown()
            .filter { it.isFile }
            .sumOf { it.length() }
    }
    
    /**
     * 清空目录
     */
    suspend fun clearDirectory(directory: File) = withContext(Dispatchers.IO) {
        if (directory.exists()) {
            directory.listFiles()?.forEach { it.delete() }
        }
    }
    
    /**
     * 获取所有数据文件的总大小
     */
    suspend fun getTotalDataSize(): Long = withContext(Dispatchers.IO) {
        getDirectorySize(dataRootDir)
    }
    
    /**
     * 获取所有缓存文件的总大小
     */
    suspend fun getTotalCacheSize(): Long = withContext(Dispatchers.IO) {
        getDirectorySize(cacheRootDir)
    }
}
