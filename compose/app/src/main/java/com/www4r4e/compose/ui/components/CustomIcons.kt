package com.www4r4e.compose.ui.components

import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.PathFillType
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.graphics.vector.path
import androidx.compose.ui.unit.dp

object GitIcons {
    val CallSplit: ImageVector by lazy { ImageVector.Builder("CallSplit", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(17f, 4f); lineTo(21f, 8f); lineTo(17f, 12f); verticalLineTo(9f)
            horizontalLineTo(12.94f); lineTo(8.38f, 4.62f)
            curveTo(8f, 4.23f, 7.48f, 4f, 6.92f, 4f); horizontalLineTo(3f); verticalLineTo(6f)
            horizontalLineTo(6.92f); lineTo(12.37f, 11.45f); lineTo(9.16f, 16.71f)
            curveTo(8.53f, 17.79f, 8.79f, 19.16f, 9.74f, 19.96f)
            curveTo(10.7f, 20.76f, 12.12f, 20.73f, 13.04f, 19.89f)
            curveTo(13.96f, 19.05f, 14.19f, 17.65f, 13.55f, 16.53f)
            lineTo(11.55f, 18.16f); lineTo(14.72f, 12.84f); lineTo(17f, 15f)
            verticalLineTo(9f); horizontalLineTo(21f); verticalLineTo(4f); close()
            moveTo(7f, 20f); curveTo(6.45f, 20f, 6f, 19.55f, 6f, 19f)
            curveTo(6f, 18.45f, 6.45f, 18f, 7f, 18f)
            curveTo(7.55f, 18f, 8f, 18.45f, 8f, 19f)
            curveTo(8f, 19.55f, 7.55f, 20f, 7f, 20f); close()
        }
    }.build() }

    val Explore: ImageVector by lazy { ImageVector.Builder("Explore", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(12f, 10.9f)
            curveTo(11.39f, 10.9f, 10.9f, 11.39f, 10.9f, 12f)
            curveTo(10.9f, 12.61f, 11.39f, 13.1f, 12f, 13.1f)
            curveTo(12.61f, 13.1f, 13.1f, 12.61f, 13.1f, 12f)
            curveTo(13.1f, 11.39f, 12.61f, 10.9f, 12f, 10.9f); close()
            moveTo(12f, 2f)
            curveTo(6.48f, 2f, 2f, 6.48f, 2f, 12f)
            curveTo(2f, 17.52f, 6.48f, 22f, 12f, 22f)
            curveTo(17.52f, 22f, 22f, 17.52f, 22f, 12f)
            curveTo(22f, 6.48f, 17.52f, 2f, 12f, 2f); close()
            moveTo(14.19f, 14.19f); lineTo(6f, 18f)
            lineTo(9.81f, 9.81f); lineTo(18f, 6f); lineTo(14.19f, 14.19f); close()
        }
    }.build() }

    val History: ImageVector by lazy { ImageVector.Builder("History", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(13f, 3f); curveTo(7.5f, 3f, 3f, 7.5f, 3f, 13f)
            horizontalLineTo(1f); lineTo(4.89f, 16.89f); lineTo(4.96f, 17.03f)
            lineTo(9f, 13f); horizontalLineTo(6f); curveTo(6f, 9.13f, 9.13f, 6f, 13f, 6f)
            curveTo(16.87f, 6f, 20f, 9.13f, 20f, 13f)
            curveTo(20f, 16.87f, 16.87f, 20f, 13f, 20f)
            curveTo(11.07f, 20f, 9.32f, 19.21f, 8.06f, 17.94f)
            lineTo(6.64f, 19.36f)
            curveTo(8.27f, 20.99f, 10.5f, 22f, 13f, 22f)
            curveTo(18.5f, 22f, 23f, 17.5f, 23f, 13f)
            curveTo(23f, 8.5f, 18.5f, 3f, 13f, 3f); close()
            moveTo(12f, 8f); verticalLineTo(13f); lineTo(16.28f, 15.54f)
            lineTo(17f, 14.33f); lineTo(13.5f, 12.25f); verticalLineTo(8f); close()
        }
    }.build() }

    val FilterList: ImageVector by lazy { ImageVector.Builder("FilterList", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(10f, 18f); horizontalLineTo(14f); verticalLineTo(16f)
            horizontalLineTo(10f); verticalLineTo(18f); close()
            moveTo(3f, 6f); verticalLineTo(8f); horizontalLineTo(21f); verticalLineTo(6f)
            horizontalLineTo(3f); close()
            moveTo(6f, 13f); horizontalLineTo(18f); verticalLineTo(11f); horizontalLineTo(6f)
            verticalLineTo(13f); close()
        }
    }.build() }

    val DarkMode: ImageVector by lazy { ImageVector.Builder("DarkMode", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            // Outer arc (full circle) then inner arc (offset) via cubic beziers
            moveTo(12f, 2f)
            curveTo(6.48f, 2f, 2f, 6.48f, 2f, 12f)
            curveTo(2f, 17.52f, 6.48f, 22f, 12f, 22f)
            curveTo(17.52f, 22f, 22f, 17.52f, 22f, 12f)
            curveTo(22f, 6.48f, 17.52f, 2f, 12f, 2f)
            close()
            // Inner crescent cutout
            moveTo(14f, 4f)
            curveTo(10f, 4f, 6f, 7f, 6f, 12f)
            curveTo(6f, 17f, 10f, 20f, 14f, 20f)
            curveTo(17f, 20f, 19f, 18f, 19f, 16f)
            curveTo(16f, 18f, 13f, 18f, 11f, 16f)
            curveTo(9f, 14f, 9f, 10f, 11f, 8f)
            curveTo(13f, 6f, 16f, 6f, 19f, 8f)
            curveTo(19f, 6f, 17f, 4f, 14f, 4f)
            close()
        }
    }.build() }

    val Key: ImageVector by lazy { ImageVector.Builder("Key", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.NonZero) {
            moveTo(12f, 8f)
            curveTo(10.9f, 8f, 10f, 8.9f, 10f, 10f)
            // Simplified key: circle head + shaft + teeth
            moveTo(7f, 13f)
            curveTo(5.34f, 13f, 4f, 11.66f, 4f, 10f)
            curveTo(4f, 8.34f, 5.34f, 7f, 7f, 7f)
            curveTo(8.66f, 7f, 10f, 8.34f, 10f, 10f)
            curveTo(10f, 10.67f, 9.77f, 11.29f, 9.38f, 11.78f)
            lineTo(13f, 11.78f); lineTo(13f, 13f); verticalLineTo(15f)
            lineTo(15f, 15f); lineTo(15f, 13f); lineTo(17f, 13f)
            lineTo(17f, 10f); lineTo(11.78f, 10f)
            close()
            // Shaft
            moveTo(13f, 13f); lineTo(18f, 13f)
            lineTo(18f, 10f); lineTo(13f, 10f); close()
            // Teeth
            lineTo(17f, 8f); lineTo(19f, 8f); lineTo(19f, 10f); close()
            moveTo(17f, 13f); lineTo(19f, 13f)
            lineTo(19f, 15f); lineTo(17f, 15f); close()
        }
    }.build() }

    val BookmarkBorder: ImageVector by lazy { ImageVector.Builder("BookmarkBorder", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(17f, 3f); horizontalLineTo(7f)
            curveTo(5.9f, 3f, 5f, 3.9f, 5f, 5f); lineTo(5f, 21f)
            lineTo(12f, 18f); lineTo(19f, 21f); lineTo(19f, 5f)
            curveTo(19f, 3.9f, 18.1f, 3f, 17f, 3f); close()
            moveTo(17f, 18f); lineTo(12f, 15.82f); lineTo(7f, 18f)
            lineTo(7f, 5f); horizontalLineTo(17f); verticalLineTo(18f); close()
        }
    }.build() }

    val Download: ImageVector by lazy { ImageVector.Builder("Download", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(19f, 9f); horizontalLineTo(15f); verticalLineTo(3f)
            horizontalLineTo(9f); verticalLineTo(9f); horizontalLineTo(5f)
            lineTo(12f, 16f); lineTo(19f, 9f); close()
            moveTo(5f, 18f); verticalLineTo(20f); horizontalLineTo(19f)
            verticalLineTo(18f); horizontalLineTo(5f); close()
        }
    }.build() }

    val DoneAll: ImageVector by lazy { ImageVector.Builder("DoneAll", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(18f, 7f); lineTo(16.59f, 5.59f)
            lineTo(6.18f, 16.18f); lineTo(2.41f, 12.41f)
            lineTo(1f, 13.82f); lineTo(6.18f, 19f); lineTo(18f, 7f); close()
            moveTo(23.01f, 6.99f); lineTo(21.6f, 5.58f)
            lineTo(15.43f, 11.75f); lineTo(16.84f, 13.16f)
            lineTo(23.01f, 6.99f); close()
        }
    }.build() }

    val NewReleases: ImageVector by lazy { ImageVector.Builder("NewReleases", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(23f, 12f); lineTo(20.56f, 9.22f); lineTo(20.9f, 5.54f)
            lineTo(17.29f, 4.72f); lineTo(15.4f, 1.54f); lineTo(12f, 3f)
            lineTo(8.6f, 1.54f); lineTo(6.71f, 4.72f); lineTo(3.1f, 5.53f)
            lineTo(3.44f, 9.22f); lineTo(1f, 12f); lineTo(3.44f, 14.78f)
            lineTo(3.1f, 18.47f); lineTo(6.71f, 19.28f); lineTo(8.6f, 22.46f)
            lineTo(12f, 21f); lineTo(15.4f, 22.46f); lineTo(17.29f, 19.28f)
            lineTo(20.9f, 18.46f); lineTo(20.56f, 14.78f); lineTo(23f, 12f); close()
            moveTo(13f, 17f); horizontalLineTo(11f); verticalLineTo(15f)
            horizontalLineTo(13f); verticalLineTo(17f); close()
            moveTo(13f, 13f); horizontalLineTo(11f); verticalLineTo(7f)
            horizontalLineTo(13f); verticalLineTo(13f); close()
        }
    }.build() }

    val Link: ImageVector by lazy { ImageVector.Builder("Link", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(3.9f, 12f)
            curveTo(3.9f, 10.29f, 5.29f, 8.9f, 7f, 8.9f)
            horizontalLineTo(11f); verticalLineTo(7f)
            horizontalLineTo(7f); curveTo(4.24f, 7f, 2f, 9.24f, 2f, 12f)
            curveTo(2f, 14.76f, 4.24f, 17f, 7f, 17f)
            horizontalLineTo(11f); verticalLineTo(15.1f)
            horizontalLineTo(7f); curveTo(5.29f, 15.1f, 3.9f, 13.71f, 3.9f, 12f); close()
            moveTo(8f, 13f); horizontalLineTo(16f); verticalLineTo(11f)
            horizontalLineTo(8f); verticalLineTo(13f); close()
            moveTo(17f, 7f); horizontalLineTo(13f); verticalLineTo(8.9f)
            horizontalLineTo(17f); curveTo(18.71f, 8.9f, 20.1f, 10.29f, 20.1f, 12f)
            curveTo(20.1f, 13.71f, 18.71f, 15.1f, 17f, 15.1f)
            horizontalLineTo(13f); verticalLineTo(17f)
            horizontalLineTo(17f); curveTo(19.76f, 17f, 22f, 14.76f, 22f, 12f)
            curveTo(22f, 9.24f, 19.76f, 7f, 17f, 7f); close()
        }
    }.build() }

    val OpenInBrowser: ImageVector by lazy { ImageVector.Builder("OpenInBrowser", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(19f, 4f); horizontalLineTo(5f)
            curveTo(3.89f, 4f, 3f, 4.9f, 3f, 6f)
            verticalLineTo(18f); curveTo(3f, 19.1f, 3.89f, 20f, 5f, 20f)
            horizontalLineTo(9f); verticalLineTo(18f); horizontalLineTo(5f)
            verticalLineTo(8f); horizontalLineTo(19f); verticalLineTo(18f)
            horizontalLineTo(15f); verticalLineTo(20f); horizontalLineTo(19f)
            curveTo(20.11f, 20f, 21f, 19.1f, 21f, 18f)
            verticalLineTo(6f); curveTo(21f, 4.9f, 20.11f, 4f, 19f, 4f); close()
            moveTo(12f, 10f); lineTo(8f, 14f); horizontalLineTo(11f)
            verticalLineTo(20f); horizontalLineTo(13f); verticalLineTo(14f)
            horizontalLineTo(16f); lineTo(12f, 10f); close()
        }
    }.build() }

    val Whatshot: ImageVector by lazy { ImageVector.Builder("Whatshot", 24.dp, 24.dp, 24f, 24f).apply {
        path(fill = SolidColor(Color.Black), pathFillType = PathFillType.EvenOdd) {
            moveTo(13.5f, 0.67f)
            curveTo(13.5f, 0.67f, 14.24f, 3.32f, 14.24f, 5.47f)
            curveTo(14.24f, 7.53f, 12.89f, 9.2f, 10.83f, 9.2f)
            curveTo(8.76f, 9.2f, 7.2f, 7.53f, 7.2f, 5.47f)
            lineTo(7.23f, 5.11f)
            curveTo(5.21f, 7.51f, 4f, 10.62f, 4f, 14f)
            curveTo(4f, 18.42f, 7.58f, 22f, 12f, 22f)
            curveTo(16.42f, 22f, 20f, 18.42f, 20f, 14f)
            curveTo(20f, 8.61f, 17.41f, 3.8f, 13.5f, 0.67f); close()
            moveTo(11.71f, 19.99f)
            curveTo(10.39f, 19.99f, 9.12f, 19.47f, 8.17f, 18.53f)
            curveTo(7.22f, 17.58f, 6.69f, 16.31f, 6.69f, 14.99f)
            curveTo(6.69f, 13.66f, 7.21f, 12.39f, 8.15f, 11.45f)
            curveTo(8.55f, 11.01f, 8.92f, 10.7f, 9.53f, 10.39f)
            curveTo(9.37f, 11.03f, 9.28f, 11.7f, 9.28f, 12.37f)
            curveTo(9.28f, 14.75f, 10.54f, 16.82f, 12.44f, 17.95f)
            curveTo(12.63f, 18.06f, 12.82f, 18.18f, 13f, 18.31f)
            curveTo(12.73f, 18.51f, 12.46f, 18.66f, 12.14f, 18.78f)
            curveTo(11.72f, 18.94f, 11.26f, 19.02f, 10.8f, 19.02f)
            lineTo(11.71f, 19.99f); close()
        }
    }.build() }
}
