package com.www4r4e.compose.ui

import androidx.lifecycle.ViewModel
import com.www4r4e.compose.data.mock.MockUser
import com.www4r4e.compose.data.model.GitHubUser
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class UserViewModel : ViewModel() {
    private val _currentUser = MutableStateFlow(MockUser.currentUser)
    val currentUser: StateFlow<GitHubUser> = _currentUser.asStateFlow()
}
