package com.www4r4e.compose.ui.notifications

import androidx.lifecycle.ViewModel
import com.www4r4e.compose.data.mock.MockNotifications
import com.www4r4e.compose.data.model.Notification
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

data class NotificationsUiState(
    val notifications: List<Notification> = emptyList(),
    val showUnreadOnly: Boolean = false
)

class NotificationsViewModel : ViewModel() {
    private val _uiState = MutableStateFlow(NotificationsUiState())
    val uiState: StateFlow<NotificationsUiState> = _uiState.asStateFlow()

    val filteredNotifications: List<Notification>
        get() {
            val state = _uiState.value
            return if (state.showUnreadOnly)
                state.notifications.filter { it.isUnread }
            else
                state.notifications
        }

    init {
        _uiState.value = NotificationsUiState(
            notifications = MockNotifications.notifications
        )
    }

    fun toggleFilter() {
        _uiState.value = _uiState.value.copy(showUnreadOnly = !_uiState.value.showUnreadOnly)
    }

    fun markAllRead() {
        _uiState.value = _uiState.value.copy(
            notifications = _uiState.value.notifications.map { it.copy(isUnread = false) }
        )
    }
}
