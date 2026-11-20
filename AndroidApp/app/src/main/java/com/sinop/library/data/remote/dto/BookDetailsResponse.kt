package com.sinop.library.data.remote.dto

data class BookCopySummary(
    val id: Int,
    val accessionNumber: String,
    val status: Int, // C# CopyStatus enum serializes as int (0=Available, 1=Borrowed, ...)
    val bookId: Int,
    val bookTitle: String
)

data class BookDetailsResponse(
    val id: Int,
    val title: String,
    val isbn: String,
    val summary: String?,
    val categoryId: Int?,
    val categoryName: String?,
    val authors: List<String>,
    val copies: List<BookCopySummary>
)
