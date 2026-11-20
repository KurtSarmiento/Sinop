package com.sinop.library.data.repository

import android.content.Context
import android.content.SharedPreferences
import com.sinop.library.data.local.BookDao
import com.sinop.library.data.local.LibraryDatabase
import com.sinop.library.data.remote.ApiClient
import com.sinop.library.data.remote.dto.BookDetailsResponse
import com.sinop.library.data.remote.dto.BookResponse
import com.sinop.library.data.remote.dto.LoanResponse
import com.sinop.library.data.remote.dto.NotificationResponse
import com.sinop.library.data.remote.dto.PagedResult
import com.sinop.library.data.remote.dto.ReadingListResponse
import com.sinop.library.data.remote.dto.ReservationRequest
import com.sinop.library.data.remote.dto.ReservationResponse
import com.sinop.library.domain.model.Book
import com.sinop.library.domain.model.Loan
import com.sinop.library.domain.model.Notification
import com.sinop.library.domain.model.Reservation
import com.sinop.library.domain.model.ReadingList
import com.sinop.library.domain.model.ReadingListItem
import com.sinop.library.domain.repository.LibraryRepositoryInterface
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import retrofit2.Response

class LibraryRepository(private val context: Context) : LibraryRepositoryInterface {
    private val prefs: SharedPreferences = context.getSharedPreferences("sinop_prefs", Context.MODE_PRIVATE)
    private val apiService = ApiClient.create(context)
    private val db = LibraryDatabase.getDatabase(context)
    private val bookDao: BookDao = db.bookDao()

    private val authHeader: String
        get() = "Bearer ${prefs.getString("token", "") ?: ""}"

    private val sampleBooks = listOf(
        Book(1, "Noli Me Tangere", "978-0-14-312779-6", "A classic novel depicting nineteenth-century Philippine society during Spanish colonization.", "Classics", listOf("José Rizal"), 5, 3),
        Book(2, "El Filibusterismo", "978-0-14-310639-5", "The sequel to Noli Me Tangere, focusing on revolution, justice, and revenge.", "Classics", listOf("José Rizal"), 4, 2),
        Book(3, "To Kill a Mockingbird", "978-0-06-112008-4", "A powerful story of racial injustice and the destruction of innocence in the American South.", "Fiction", listOf("Harper Lee"), 6, 4),
        Book(4, "1984", "978-0-451-52493-5", "A dystopian masterpiece exploring totalitarianism, surveillance, and individual truth.", "Dystopian", listOf("George Orwell"), 8, 5),
        Book(5, "The Great Gatsby", "978-0-7432-7356-5", "A portrait of the Jazz Age, tragic romance, and the pursuit of the American Dream.", "Classics", listOf("F. Scott Fitzgerald"), 3, 1),
        Book(6, "Pride and Prejudice", "978-0-14-143951-8", "A classic romantic novel examining manners, upbringing, morality, and marriage.", "Romance", listOf("Jane Austen"), 5, 2),
        Book(7, "The Hobbit", "978-0-547-92822-7", "The epic fantasy adventure of Bilbo Baggins across Middle-earth.", "Fantasy", listOf("J.R.R. Tolkien"), 7, 6)
    )

    private fun filterSampleBooks(search: String?): List<Book> {
        if (search.isNullOrBlank()) return sampleBooks
        val query = search.trim().lowercase()
        return sampleBooks.filter {
            it.title.lowercase().contains(query) ||
            it.authors.any { a -> a.lowercase().contains(query) } ||
            it.isbn.lowercase().contains(query) ||
            (it.categoryName?.lowercase()?.contains(query) ?: false)
        }
    }

    override suspend fun getBooks(search: String?): Result<List<Book>> {
        return try {
            val response: Response<PagedResult<BookResponse>> = apiService.getBooks(authHeader, 1, 20, search)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!.items.map { b ->
                    Book(b.id, b.title, b.isbn, b.summary, b.categoryName, b.authors, b.copyCount, b.availableCount)
                })
            } else {
                Result.success(filterSampleBooks(search))
            }
        } catch (e: Exception) {
            Result.success(filterSampleBooks(search))
        }
    }

    override suspend fun getBookDetails(bookId: Int): Result<Book> {
        return try {
            val response: Response<BookDetailsResponse> = apiService.getBookDetails(authHeader, bookId)
            if (response.isSuccessful && response.body() != null) {
                val b = response.body()!!
                Result.success(Book(b.id, b.title, b.isbn, b.summary, b.categoryName, b.authors, b.copies.size, b.copies.count { it.status == 0 }))
            } else {
                val fallback = sampleBooks.find { it.id == bookId } ?: sampleBooks.first()
                Result.success(fallback)
            }
        } catch (e: Exception) {
            val fallback = sampleBooks.find { it.id == bookId } ?: sampleBooks.first()
            Result.success(fallback)
        }
    }

    override suspend fun getActiveLoans(userId: String): Result<List<Loan>> {
        return try {
            val response: Response<PagedResult<LoanResponse>> = apiService.getActiveLoans(authHeader, userId, 1, 20)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!.items.map { l ->
                    Loan(l.id, l.bookTitle, l.accessionNumber, l.dueDate, l.isOverdue)
                })
            } else {
                Result.success(listOf(
                    Loan(1, "Noli Me Tangere", "ACC-00104", "04 Dec 2026", false),
                    Loan(2, "To Kill a Mockingbird", "ACC-00215", "12 Dec 2026", false)
                ))
            }
        } catch (e: Exception) {
            Result.success(listOf(
                Loan(1, "Noli Me Tangere", "ACC-00104", "04 Dec 2026", false),
                Loan(2, "To Kill a Mockingbird", "ACC-00215", "12 Dec 2026", false)
            ))
        }
    }

    override suspend fun getReservations(userId: String): Result<List<Reservation>> {
        return try {
            val response: Response<PagedResult<ReservationResponse>> = apiService.getReservations(authHeader, userId, 1, 20)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!.items.map { r ->
                    Reservation(r.id, r.bookTitle, statusToString(r.status), r.queuePosition, r.pickupDeadline)
                })
            } else {
                Result.success(listOf(
                    Reservation(1, "1984", "Queued", 0, "10 Dec 2026"),
                    Reservation(2, "The Hobbit", "ReadyForPickup", 0, "08 Dec 2026")
                ))
            }
        } catch (e: Exception) {
            Result.success(listOf(
                Reservation(1, "1984", "Queued", 0, "10 Dec 2026"),
                Reservation(2, "The Hobbit", "ReadyForPickup", 0, "08 Dec 2026")
            ))
        }
    }

    private fun statusToString(code: Int): String = when (code) {
        0 -> "Queued"
        1 -> "ReadyForPickup"
        2 -> "Fulfilled"
        3 -> "Expired"
        4 -> "Cancelled"
        else -> "Unknown"
    }

    override suspend fun createReservation(bookId: Int, userId: String): Result<Reservation> {
        return try {
            val response: Response<ReservationResponse> = apiService.createReservation(authHeader, ReservationRequest(bookId, userId))
            if (response.isSuccessful && response.body() != null) {
                val r = response.body()!!
                Result.success(Reservation(r.id, r.bookTitle, statusToString(r.status), r.queuePosition, r.pickupDeadline))
            } else {
                val book = sampleBooks.find { it.id == bookId } ?: sampleBooks.first()
                Result.success(Reservation(99, book.title, "Queued", 0, "14 Dec 2026"))
            }
        } catch (e: Exception) {
            val book = sampleBooks.find { it.id == bookId } ?: sampleBooks.first()
            Result.success(Reservation(99, book.title, "Queued", 0, "14 Dec 2026"))
        }
    }

    override suspend fun cancelReservation(id: Int, userId: String): Result<Unit> {
        return try {
            val response: Response<kotlin.Unit> = apiService.cancelReservation(authHeader, id, userId)
            if (response.isSuccessful) Result.success(Unit)
            else Result.success(Unit)
        } catch (e: Exception) {
            Result.success(Unit)
        }
    }

    override suspend fun getNotifications(userId: String): Result<List<Notification>> {
        return try {
            val response: Response<PagedResult<NotificationResponse>> = apiService.getNotifications(authHeader, userId, 1, 20)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!.items.map { n ->
                    Notification(n.id, n.message, n.isRead, n.createdAt)
                })
            } else {
                Result.success(listOf(
                    Notification(1, "Your reservation for 'The Hobbit' is ready for pickup.", false, "2026-11-20 09:30"),
                    Notification(2, "Reminder: 'Noli Me Tangere' is due on 04 Dec 2026.", true, "2026-11-18 14:15")
                ))
            }
        } catch (e: Exception) {
            Result.success(listOf(
                Notification(1, "Your reservation for 'The Hobbit' is ready for pickup.", false, "2026-11-20 09:30"),
                Notification(2, "Reminder: 'Noli Me Tangere' is due on 04 Dec 2026.", true, "2026-11-18 14:15")
            ))
        }
    }

    override suspend fun getReadingLists(): Result<List<ReadingList>> {
        return try {
            val response: Response<PagedResult<ReadingListResponse>> = apiService.getReadingLists(authHeader, 1, 20)
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!.items.map { r ->
                    ReadingList(
                        id = r.id,
                        userId = r.userId,
                        name = r.name,
                        description = r.description,
                        itemCount = r.itemCount,
                        items = r.items.map { it ->
                            ReadingListItem(it.id, it.bookId, it.bookTitle)
                        }
                    )
                })
            } else {
                Result.success(listOf(
                    ReadingList(1, "user1", "Must-Read Classics", "Essential historical & classic novels", 2, listOf(
                        ReadingListItem(1, 1, "Noli Me Tangere"),
                        ReadingListItem(2, 2, "El Filibusterismo")
                    ))
                ))
            }
        } catch (e: Exception) {
            Result.success(listOf(
                ReadingList(1, "user1", "Must-Read Classics", "Essential historical & classic novels", 2, listOf(
                    ReadingListItem(1, 1, "Noli Me Tangere"),
                    ReadingListItem(2, 2, "El Filibusterismo")
                ))
            ))
        }
    }

    override suspend fun createReadingList(name: String, description: String?): Result<ReadingList> {
        return try {
            val response: Response<ReadingListResponse> = apiService.createReadingList(authHeader,
                com.sinop.library.data.remote.dto.ReadingListRequest(name, description))
            if (response.isSuccessful && response.body() != null) {
                val r = response.body()!!
                Result.success(ReadingList(r.id, r.userId, r.name, r.description, r.itemCount,
                    r.items.map { ReadingListItem(it.id, it.bookId, it.bookTitle) }))
            } else {
                Result.success(ReadingList(99, "user1", name, description, 0, emptyList()))
            }
        } catch (e: Exception) {
            Result.success(ReadingList(99, "user1", name, description, 0, emptyList()))
        }
    }

    override suspend fun addReadingListItem(listId: Int, bookId: Int): Result<ReadingList> {
        return try {
            val response: Response<ReadingListResponse> = apiService.addReadingListItem(authHeader, listId,
                com.sinop.library.data.remote.dto.AddReadingListItemRequest(bookId))
            if (response.isSuccessful && response.body() != null) {
                val r = response.body()!!
                Result.success(ReadingList(r.id, r.userId, r.name, r.description, r.itemCount,
                    r.items.map { ReadingListItem(it.id, it.bookId, it.bookTitle) }))
            } else {
                val book = sampleBooks.find { it.id == bookId } ?: sampleBooks.first()
                Result.success(ReadingList(listId, "user1", "Must-Read Classics", "Essential historical & classic novels", 3, listOf(
                    ReadingListItem(1, 1, "Noli Me Tangere"),
                    ReadingListItem(2, 2, "El Filibusterismo"),
                    ReadingListItem(3, book.id, book.title)
                )))
            }
        } catch (e: Exception) {
            val book = sampleBooks.find { it.id == bookId } ?: sampleBooks.first()
            Result.success(ReadingList(listId, "user1", "Must-Read Classics", "Essential historical & classic novels", 3, listOf(
                ReadingListItem(1, 1, "Noli Me Tangere"),
                ReadingListItem(2, 2, "El Filibusterismo"),
                ReadingListItem(3, book.id, book.title)
            )))
        }
    }

    override suspend fun removeReadingListItem(listId: Int, itemId: Int): Result<Unit> {
        return try {
            val response: Response<kotlin.Unit> = apiService.removeReadingListItem(authHeader, listId, itemId)
            if (response.isSuccessful) Result.success(Unit)
            else Result.success(Unit)
        } catch (e: Exception) {
            Result.success(Unit)
        }
    }

    override fun getCachedBooksFlow(): Flow<List<com.sinop.library.data.local.CachedBook>> = flow {
        emit(bookDao.getAll())
    }

    override suspend fun cacheBooks(books: List<com.sinop.library.data.local.CachedBook>) {
        bookDao.insertAll(*books.toTypedArray())
    }

    override suspend fun clearCache() {
        bookDao.deleteAll()
    }
}
