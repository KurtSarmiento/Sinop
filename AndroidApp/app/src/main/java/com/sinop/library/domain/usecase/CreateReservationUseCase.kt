package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.Reservation
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class CreateReservationUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(bookId: Int, userId: String): Result<Reservation> {
        return libraryRepository.createReservation(bookId, userId)
    }
}
