package com.sinop.library.di

import android.content.Context
import com.sinop.library.data.repository.AuthRepository
import com.sinop.library.data.repository.LibraryRepository
import com.sinop.library.domain.repository.LibraryRepositoryInterface
import com.sinop.library.domain.usecase.AddReadingListItemUseCase
import com.sinop.library.domain.usecase.CreateReservationUseCase
import com.sinop.library.domain.usecase.CreateReadingListUseCase
import com.sinop.library.domain.usecase.CancelReservationUseCase
import com.sinop.library.domain.usecase.GetActiveLoansUseCase
import com.sinop.library.domain.usecase.GetBookDetailsUseCase
import com.sinop.library.domain.usecase.GetBooksUseCase
import com.sinop.library.domain.usecase.GetNotificationsUseCase
import com.sinop.library.domain.usecase.GetReadingListsUseCase
import com.sinop.library.domain.usecase.GetReservationsUseCase
import com.sinop.library.domain.usecase.LoginUseCase
import com.sinop.library.domain.usecase.RemoveReadingListItemUseCase

class ServiceLocator private constructor(
    val authRepository: AuthRepository,
    val libraryRepository: LibraryRepositoryInterface,
    val loginUseCase: LoginUseCase,
    val getBooksUseCase: GetBooksUseCase,
    val getBookDetailsUseCase: GetBookDetailsUseCase,
    val getActiveLoansUseCase: GetActiveLoansUseCase,
    val getReservationsUseCase: GetReservationsUseCase,
    val createReservationUseCase: CreateReservationUseCase,
    val cancelReservationUseCase: CancelReservationUseCase,
    val getReadingListsUseCase: GetReadingListsUseCase,
    val createReadingListUseCase: CreateReadingListUseCase,
    val addReadingListItemUseCase: AddReadingListItemUseCase,
    val removeReadingListItemUseCase: RemoveReadingListItemUseCase,
    val getNotificationsUseCase: GetNotificationsUseCase
) {
    companion object {
        @Volatile private var INSTANCE: ServiceLocator? = null

        fun init(context: Context): ServiceLocator {
            return INSTANCE ?: synchronized(this) {
                val authRepo = AuthRepository(context)
                val libRepo: LibraryRepositoryInterface = LibraryRepository(context)
                val instance = ServiceLocator(
                    authRepo,
                    libRepo,
                    LoginUseCase(authRepo),
                    GetBooksUseCase(libRepo),
                    GetBookDetailsUseCase(libRepo),
                    GetActiveLoansUseCase(libRepo),
                    GetReservationsUseCase(libRepo),
                    CreateReservationUseCase(libRepo),
                    CancelReservationUseCase(libRepo),
                    GetReadingListsUseCase(libRepo),
                    CreateReadingListUseCase(libRepo),
                    AddReadingListItemUseCase(libRepo),
                    RemoveReadingListItemUseCase(libRepo),
                    GetNotificationsUseCase(libRepo)
                )
                INSTANCE = instance
                instance
            }
        }

        fun get(): ServiceLocator = INSTANCE ?: throw IllegalStateException("ServiceLocator not initialized")
    }
}
