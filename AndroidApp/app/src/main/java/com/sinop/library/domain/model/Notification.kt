package com.sinop.library.domain.model

data class Notification(
    val id: Int,
    val message: String,
    val isRead: Boolean,
    val createdAt: String
)
