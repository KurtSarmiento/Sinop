package com.sinop.library.data.remote.dto

data class BookCopyResponse(
    val id: Int,
    val accessionNumber: String,
    val status: Int,
    val bookId: Int,
    val bookTitle: String
)
