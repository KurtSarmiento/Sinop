package com.sinop.library.domain.model

data class ReadingList(
    val id: Int,
    val userId: String,
    val name: String,
    val description: String?,
    val itemCount: Int,
    val items: List<ReadingListItem>
)
