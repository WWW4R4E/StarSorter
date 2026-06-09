package com.www4r4e.compose.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.www4r4e.compose.ui.theme.*

fun languageColor(language: String?): Color = when (language?.lowercase()) {
    "kotlin" -> LanguageDotKotlin
    "python" -> LanguageDotPython
    "javascript" -> LanguageDotJavaScript
    "typescript" -> LanguageDotTypeScript
    "rust" -> LanguageDotRust
    "go" -> LanguageDotGo
    "java" -> LanguageDotJava
    "swift" -> LanguageDotSwift
    "dart" -> LanguageDotDart
    "ruby" -> LanguageDotRuby
    "c-plus-plus", "c++" -> LanguageDotCpp
    "c-sharp", "c#" -> LanguageDotCSharp
    else -> Color.Gray
}

@Composable
fun LanguageDot(
    language: String?,
    modifier: Modifier = Modifier,
    dotSize: Dp = 10.dp
) {
    Box(
        modifier = modifier
            .size(dotSize)
            .clip(CircleShape)
            .background(languageColor(language))
    )
}
