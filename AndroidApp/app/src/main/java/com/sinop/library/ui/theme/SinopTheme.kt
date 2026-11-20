package com.sinop.library.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import com.sinop.library.ui.components.*

private val DarkColorScheme = darkColorScheme(
    primary = Color(0xFFD9B552),
    onPrimary = Color(0xFF3F0F19),
    primaryContainer = Color(0xFF3F0F19),
    onPrimaryContainer = Color(0xFFF3EBDC),
    secondary = Color(0xFFD9B552),
    onSecondary = Color(0xFF3F0F19),
    tertiary = Color(0xFFC8A038),
    background = Color(0xFF1E0C10),
    onBackground = Color(0xFFF3EBDC),
    surface = Color(0xFF2A1419),
    onSurface = Color(0xFFF3EBDC),
    surfaceVariant = Color(0xFF3A1A22),
    onSurfaceVariant = Color(0xFFBBA8A6),
    outline = Color(0xFF4A2A31),
    error = Color(0xFFD32F2F),
    onError = Color(0xFFFFFFFF)
)

private val LightColorScheme = lightColorScheme(
    primary = SinopMaroon,
    onPrimary = SinopPaper,
    primaryContainer = SinopMaroon,
    onPrimaryContainer = SinopPaper,
    secondary = SinopGold,
    onSecondary = SinopDeepMaroon,
    secondaryContainer = SinopGold,
    onSecondaryContainer = SinopDeepMaroon,
    tertiary = SinopGold,
    onTertiary = SinopDeepMaroon,
    background = SinopPaper,
    onBackground = SinopInk,
    surface = SinopCard,
    onSurface = SinopInk,
    surfaceVariant = Color(0xFFEDE4D3),
    onSurfaceVariant = SinopMuted,
    outline = SinopLine,
    error = SinopMaroon,
    onError = SinopPaper
)

@Composable
fun SinopTheme(
    darkTheme: Boolean = false,
    content: @Composable () -> Unit
) {
    val colorScheme = if (!darkTheme) LightColorScheme else DarkColorScheme
    MaterialTheme(
        colorScheme = colorScheme,
        typography = MaterialTheme.typography,
        shapes = MaterialTheme.shapes,
        content = content
    )
}
