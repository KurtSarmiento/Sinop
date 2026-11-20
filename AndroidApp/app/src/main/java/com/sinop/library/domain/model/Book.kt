package com.sinop.library.domain.model

data class Book(
    val id: Int,
    val title: String,
    val isbn: String,
    val summary: String?,
    val categoryName: String?,
    val authors: List<String>,
    val copyCount: Int,
    val availableCount: Int
)
