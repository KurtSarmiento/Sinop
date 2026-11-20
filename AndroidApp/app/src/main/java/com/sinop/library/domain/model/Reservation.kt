package com.sinop.library.domain.model

data class Reservation(
    val id: Int,
    val bookTitle: String,
    val status: String,
    val queuePosition: Int,
    val pickupDeadline: String?
)
