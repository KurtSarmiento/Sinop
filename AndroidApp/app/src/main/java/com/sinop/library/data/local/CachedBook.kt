package com.sinop.library.data.local

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "cached_books")
data class CachedBook(
    @PrimaryKey val id: Int,
    val title: String,
    val isbn: String,
    val summary: String?,
    val categoryName: String?,
    val copyCount: Int,
    val availableCount: Int
)
