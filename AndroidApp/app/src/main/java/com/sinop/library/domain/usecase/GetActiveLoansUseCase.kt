package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.Loan
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class GetActiveLoansUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(userId: String): Result<List<Loan>> {
        return libraryRepository.getActiveLoans(userId)
    }
}
