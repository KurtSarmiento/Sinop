package com.sinop.library.data.remote.dto

data class ReadingListResponse(
    val id: Int,
    val userId: String,
    val name: String,
    val description: String?,
    val itemCount: Int,
    val items: List<ReadingListItemResponse>
)
