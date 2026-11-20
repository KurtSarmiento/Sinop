package com.sinop.library.data.remote.dto

data class ReadingListRequest(
    val name: String,
    val description: String? = null
)
