package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.ReadingList
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class CreateReadingListUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(name: String, description: String?): Result<ReadingList> {
        return libraryRepository.createReadingList(name, description)
    }
}
