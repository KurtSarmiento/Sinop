package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.ReadingList
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class AddReadingListItemUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(listId: Int, bookId: Int): Result<ReadingList> {
        return libraryRepository.addReadingListItem(listId, bookId)
    }
}
