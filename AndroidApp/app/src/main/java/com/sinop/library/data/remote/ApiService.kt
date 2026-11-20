package com.sinop.library.data.remote

import com.sinop.library.data.remote.dto.AddReadingListItemRequest
import com.sinop.library.data.remote.dto.AuthResponse
import com.sinop.library.data.remote.dto.BookCopyResponse
import com.sinop.library.data.remote.dto.BookDetailsResponse
import com.sinop.library.data.remote.dto.BookResponse
import com.sinop.library.data.remote.dto.LoginRequest
import com.sinop.library.data.remote.dto.LoanResponse
import com.sinop.library.data.remote.dto.PagedResult
import com.sinop.library.data.remote.dto.ReadingListItemResponse
import com.sinop.library.data.remote.dto.ReadingListResponse
import com.sinop.library.data.remote.dto.ReservationResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

interface ApiService {
    @POST("api/auth/login")
    suspend fun login(@Body request: LoginRequest): Response<AuthResponse>

    @GET("api/books")
    suspend fun getBooks(
        @Header("Authorization") authHeader: String,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("search") search: String? = null
    ): Response<PagedResult<BookResponse>>

    @GET("api/books/{id}")
    suspend fun getBookDetails(
        @Header("Authorization") authHeader: String,
        @Path("id") id: Int
    ): Response<BookDetailsResponse>

    @GET("api/bookcopies")
    suspend fun getBookCopies(
        @Header("Authorization") authHeader: String,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20
    ): Response<PagedResult<BookCopyResponse>>

    @GET("api/loans/active")
    suspend fun getActiveLoans(
        @Header("Authorization") authHeader: String,
        @Query("userId") userId: String? = null,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20
    ): Response<PagedResult<LoanResponse>>

    @GET("api/reservations")
    suspend fun getReservations(
        @Header("Authorization") authHeader: String,
        @Query("userId") userId: String? = null,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20
    ): Response<PagedResult<ReservationResponse>>

    @POST("api/reservations")
    suspend fun createReservation(
        @Header("Authorization") authHeader: String,
        @Body request: com.sinop.library.data.remote.dto.ReservationRequest
    ): Response<ReservationResponse>

    @POST("api/reservations/{id}/cancel")
    suspend fun cancelReservation(
        @Header("Authorization") authHeader: String,
        @Path("id") id: Int,
        @Query("userId") userId: String
    ): Response<kotlin.Unit>

    @GET("api/readinglists")
    suspend fun getReadingLists(
        @Header("Authorization") authHeader: String,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20
    ): Response<PagedResult<ReadingListResponse>>

    @POST("api/readinglists")
    suspend fun createReadingList(
        @Header("Authorization") authHeader: String,
        @Body request: com.sinop.library.data.remote.dto.ReadingListRequest
    ): Response<ReadingListResponse>

    @POST("api/readinglists/{listId}/items")
    suspend fun addReadingListItem(
        @Header("Authorization") authHeader: String,
        @Path("listId") listId: Int,
        @Body request: AddReadingListItemRequest
    ): Response<ReadingListResponse>

    @DELETE("api/readinglists/{listId}/items/{itemId}")
    suspend fun removeReadingListItem(
        @Header("Authorization") authHeader: String,
        @Path("listId") listId: Int,
        @Path("itemId") itemId: Int
    ): Response<kotlin.Unit>

    @GET("api/notifications")
    suspend fun getNotifications(
        @Header("Authorization") authHeader: String,
        @Query("userId") userId: String,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20
    ): Response<PagedResult<com.sinop.library.data.remote.dto.NotificationResponse>>
}
