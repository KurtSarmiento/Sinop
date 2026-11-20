package com.sinop.library.data.remote.dto

data class NotificationResponse(
    val id: Int,
    val userId: String,
    val message: String,
    val isRead: Boolean,
    val createdAt: String
)
