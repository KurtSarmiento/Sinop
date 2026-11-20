package com.sinop.library.data.remote.dto

data class LoanResponse(
    val id: Int,
    val bookCopyId: Int,
    val accessionNumber: String,
    val bookTitle: String,
    val userId: String,
    val userEmail: String,
    val loanDate: String,
    val dueDate: String,
    val returnDate: String?,
    val status: Int,
    val isOverdue: Boolean
)
