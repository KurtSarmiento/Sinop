package com.sinop.library.data.remote.dto

data class BookResponse(
    val id: Int,
    val title: String,
    val isbn: String,
    val summary: String?,
    val categoryId: Int?,
    val categoryName: String?,
    val authors: List<String>,
    val copyCount: Int,
    val availableCount: Int
)
