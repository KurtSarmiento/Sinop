package com.sinop.library.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

val SinopMaroon = Color(0xFF6D1A2B)
val SinopDeepMaroon = Color(0xFF3F0F19)
val SinopGold = Color(0xFFC8A038)
val SinopPaper = Color(0xFFF7F1E5)
val SinopCard = Color(0xFFFFFBF2)
val SinopInk = Color(0xFF2A1015)
val SinopMuted = Color(0xFF6F5A5E)
val SinopLine = Color(0xFFE2D6BE)

enum class SinopStatusType {
    AVAILABLE,
    RESERVED,
    OVERDUE
}

@Composable
fun StatusPill(
    type: SinopStatusType,
    modifier: Modifier = Modifier,
    text: String? = null
) {
    val displayText = text ?: when (type) {
        SinopStatusType.AVAILABLE -> "AVAILABLE"
        SinopStatusType.RESERVED -> "RESERVED"
        SinopStatusType.OVERDUE -> "OVERDUE"
    }

    val shape = RoundedCornerShape(999.dp)

    val boxModifier = when (type) {
        SinopStatusType.AVAILABLE -> modifier
            .border(width = 1.5.dp, color = SinopMaroon, shape = shape)
            .padding(horizontal = 10.dp, vertical = 4.dp)
        SinopStatusType.RESERVED -> modifier
            .background(color = SinopGold, shape = shape)
            .padding(horizontal = 10.dp, vertical = 4.dp)
        SinopStatusType.OVERDUE -> modifier
            .background(color = SinopMaroon, shape = shape)
            .padding(horizontal = 10.dp, vertical = 4.dp)
    }

    val textColor = when (type) {
        SinopStatusType.AVAILABLE -> SinopMaroon
        SinopStatusType.RESERVED -> SinopDeepMaroon
        SinopStatusType.OVERDUE -> SinopPaper
    }

    Box(modifier = boxModifier) {
        Text(
            text = displayText.uppercase(),
            color = textColor,
            fontSize = 11.sp,
            fontFamily = FontFamily.Monospace,
            fontWeight = FontWeight.Bold,
            letterSpacing = 0.5.sp
        )
    }
}
