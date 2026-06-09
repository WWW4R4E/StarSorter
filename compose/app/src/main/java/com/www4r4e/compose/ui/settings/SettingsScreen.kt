package com.www4r4e.compose.ui.settings

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.www4r4e.compose.ui.main.MainViewModel
import java.text.SimpleDateFormat
import java.util.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    onBack: () -> Unit,
    viewModel: MainViewModel = viewModel()
) {
    val state by viewModel.uiState.collectAsState()
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("设置") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(
                            androidx.compose.material.icons.Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = "返回"
                        )
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // 缓存统计卡片
            CacheStatsCard(state)
            
            // 缓存管理操作
            CacheManagementCard(viewModel)
            
            // 数据管理
            DataManagementCard(viewModel)
            
            // 最后同步时间
            if (state.lastSyncTime != null) {
                LastSyncCard(state.lastSyncTime!!)
            }
        }
    }
}

@Composable
private fun CacheStatsCard(state: com.www4r4e.compose.ui.main.MainUiState) {
    Card(
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Text(
                text = "缓存统计",
                style = MaterialTheme.typography.titleLarge
            )
            
            Divider()
            
            StatRow("数据大小", "${String.format("%.2f", state.cacheStats.dataSizeMB)} MB")
            StatRow("图片缓存", "${String.format("%.2f", state.cacheStats.imageCacheSizeMB)} MB")
            StatRow("图片数量", "${state.cacheStats.imageCount} 张")
            StatRow("API 缓存", "${String.format("%.2f", state.cacheStats.apiCacheSizeMB)} MB")
            
            Divider()
            
            StatRow(
                "总缓存大小",
                "${String.format("%.2f", state.cacheStats.totalCacheSizeMB)} MB",
                isHighlight = true
            )
        }
    }
}

@Composable
private fun StatRow(label: String, value: String, isHighlight: Boolean = false) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = label,
            style = if (isHighlight) MaterialTheme.typography.titleMedium
            else MaterialTheme.typography.bodyLarge
        )
        Text(
            text = value,
            style = if (isHighlight) MaterialTheme.typography.titleMedium
            else MaterialTheme.typography.bodyLarge,
            color = if (isHighlight) MaterialTheme.colorScheme.primary
            else MaterialTheme.colorScheme.onSurface
        )
    }
}

@Composable
private fun CacheManagementCard(viewModel: MainViewModel) {
    Card(
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Text(
                text = "缓存管理",
                style = MaterialTheme.typography.titleLarge
            )
            
            Divider()
            
            Button(
                onClick = { viewModel.clearImageCache() },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("清除图片缓存")
            }
            
            OutlinedButton(
                onClick = { viewModel.clearApiCache() },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("清除 API 缓存")
            }
            
            OutlinedButton(
                onClick = { viewModel.clearCache() },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("清除所有缓存")
            }
            
            TextButton(
                onClick = { viewModel.cleanupExpiredCache() },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("清理过期缓存")
            }
        }
    }
}

@Composable
private fun DataManagementCard(viewModel: MainViewModel) {
    Card(
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Text(
                text = "数据管理",
                style = MaterialTheme.typography.titleLarge,
                color = MaterialTheme.colorScheme.error
            )
            
            Divider()
            
            Text(
                text = "⚠️ 警告：此操作将删除所有本地数据，包括星标、设置等。",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.error
            )
            
            Button(
                onClick = { 
                    // TODO: 添加确认对话框
                    viewModel.clearAllData()
                },
                modifier = Modifier.fillMaxWidth(),
                colors = ButtonDefaults.buttonColors(
                    containerColor = MaterialTheme.colorScheme.error
                )
            ) {
                Text("清除所有数据")
            }
            
            OutlinedButton(
                onClick = { viewModel.refreshData() },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("刷新数据（从 Mock）")
            }
        }
    }
}

@Composable
private fun LastSyncCard(timestamp: Long) {
    Card(
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Text(
                text = "同步信息",
                style = MaterialTheme.typography.titleLarge
            )
            
            Divider()
            
            val dateFormat = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault())
            val dateStr = dateFormat.format(Date(timestamp))
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Text("最后同步时间")
                Text(
                    text = dateStr,
                    style = MaterialTheme.typography.bodyMedium,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }
        }
    }
}
