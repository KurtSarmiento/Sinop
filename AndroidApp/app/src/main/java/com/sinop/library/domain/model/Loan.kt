package com.sinop.library.domain.model

data class Loan(
    val id: Int,
    val bookTitle: String,
    val accessionNumber: String,
    val dueDate: String,
    val isOverdue: Boolean
)
