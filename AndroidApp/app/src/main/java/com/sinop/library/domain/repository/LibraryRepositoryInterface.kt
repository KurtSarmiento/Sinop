package com.sinop.library.domain.repository

import com.sinop.library.domain.model.Book
import com.sinop.library.domain.model.Loan
import com.sinop.library.domain.model.Notification
import com.sinop.library.domain.model.Reservation
import com.sinop.library.domain.model.ReadingList
import kotlinx.coroutines.flow.Flow

interface LibraryRepositoryInterface {
    suspend fun getBooks(search: String?): Result<List<Book>>
    suspend fun getBookDetails(bookId: Int): Result<Book>
    suspend fun getActiveLoans(userId: String): Result<List<Loan>>
    suspend fun getReservations(userId: String): Result<List<Reservation>>
    suspend fun createReservation(bookId: Int, userId: String): Result<Reservation>
    suspend fun cancelReservation(id: Int, userId: String): Result<Unit>
    suspend fun getNotifications(userId: String): Result<List<Notification>>
    suspend fun getReadingLists(): Result<List<ReadingList>>
    suspend fun createReadingList(name: String, description: String?): Result<ReadingList>
    suspend fun addReadingListItem(listId: Int, bookId: Int): Result<ReadingList>
    suspend fun removeReadingListItem(listId: Int, itemId: Int): Result<Unit>
    fun getCachedBooksFlow(): Flow<List<com.sinop.library.data.local.CachedBook>>
    suspend fun cacheBooks(books: List<com.sinop.library.data.local.CachedBook>)
    suspend fun clearCache()
}
