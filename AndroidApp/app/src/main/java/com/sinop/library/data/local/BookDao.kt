package com.sinop.library.data.local

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query

@Dao
interface BookDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(vararg books: CachedBook)

    @Query("DELETE FROM cached_books")
    suspend fun deleteAll()

    @Query("SELECT * FROM cached_books")
    suspend fun getAll(): List<CachedBook>
}
