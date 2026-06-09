package com.www4r4e.compose

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.www4r4e.compose.navigation.AppNavigation
import com.www4r4e.compose.ui.theme.StarSorterTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            StarSorterTheme(dynamicColor = false) {
                AppNavigation()
            }
        }
    }
}
