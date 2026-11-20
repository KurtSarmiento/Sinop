package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.ReadingList
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class GetReadingListsUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(): Result<List<ReadingList>> {
        return libraryRepository.getReadingLists()
    }
}
