package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.Reservation
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class GetReservationsUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(userId: String): Result<List<Reservation>> {
        return libraryRepository.getReservations(userId)
    }
}
