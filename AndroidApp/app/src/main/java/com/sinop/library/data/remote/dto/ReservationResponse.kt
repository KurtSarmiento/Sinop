package com.sinop.library.data.remote.dto

data class ReservationResponse(
    val id: Int,
    val bookId: Int,
    val bookTitle: String,
    val allocatedCopyId: Int?,
    val allocatedCopyAccessionNumber: String?,
    val userId: String,
    val reservedAt: String,
    val pickupDeadline: String?,
    val status: Int,
    val queuePosition: Int
)
