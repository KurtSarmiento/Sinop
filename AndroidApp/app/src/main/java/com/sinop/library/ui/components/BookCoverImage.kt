package com.sinop.library.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import coil.compose.AsyncImage

object OpenLibraryApi {
    fun getCoverUrlByIsbn(isbn: String?, size: String = "M"): String? {
        if (isbn.isNullOrBlank()) return null
        val cleanIsbn = isbn.replace("-", "").replace(" ", "").trim()
        if (cleanIsbn.isEmpty()) return null
        return "https://covers.openlibrary.org/b/isbn/$cleanIsbn-$size.jpg"
    }
}

@Composable
fun BookCoverPlaceholder(
    modifier: Modifier = Modifier,
    title: String? = null,
    width: Dp = 52.dp,
    height: Dp = 74.dp
) {
    val shape = RoundedCornerShape(topStart = 3.dp, bottomStart = 3.dp, topEnd = 6.dp, bottomEnd = 6.dp)

    Box(
        modifier = modifier
            .size(width = width, height = height)
            .clip(shape)
            .background(SinopMaroon)
    ) {
        Box(
            modifier = Modifier
                .fillMaxHeight()
                .width(5.dp)
                .background(SinopGold)
                .align(Alignment.CenterStart)
        )

        title?.let { t ->
            Text(
                text = t,
                color = SinopPaper,
                fontSize = (height.value * 0.12f).sp,
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold,
                maxLines = 3,
                overflow = TextOverflow.Ellipsis,
                textAlign = TextAlign.Center,
                modifier = Modifier
                    .fillMaxSize()
                    .padding(start = 7.dp, end = 4.dp, top = 4.dp, bottom = 4.dp)
                    .align(Alignment.Center)
            )
        }
    }
}

@Composable
fun BookCoverImage(
    isbn: String?,
    modifier: Modifier = Modifier,
    title: String? = null,
    width: Dp = 52.dp,
    height: Dp = 74.dp,
    size: String = "M"
) {
    val coverUrl = OpenLibraryApi.getCoverUrlByIsbn(isbn, size)
    val shape = RoundedCornerShape(topStart = 3.dp, bottomStart = 3.dp, topEnd = 6.dp, bottomEnd = 6.dp)

    if (!coverUrl.isNullOrEmpty()) {
        AsyncImage(
            model = coverUrl,
            contentDescription = title ?: "Book cover",
            contentScale = ContentScale.Crop,
            modifier = modifier
                .size(width = width, height = height)
                .clip(shape)
        )
    } else {
        BookCoverPlaceholder(modifier = modifier, title = title, width = width, height = height)
    }
}
