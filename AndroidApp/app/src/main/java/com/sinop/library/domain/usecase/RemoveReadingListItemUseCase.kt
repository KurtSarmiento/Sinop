package com.sinop.library.domain.usecase

import com.sinop.library.domain.repository.LibraryRepositoryInterface

class RemoveReadingListItemUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(listId: Int, itemId: Int): Result<Unit> {
        return libraryRepository.removeReadingListItem(listId, itemId)
    }
}
