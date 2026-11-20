package com.sinop.library.domain.usecase

import com.sinop.library.domain.repository.LibraryRepositoryInterface

class CancelReservationUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(id: Int, userId: String): Result<Unit> {
        return libraryRepository.cancelReservation(id, userId)
    }
}
