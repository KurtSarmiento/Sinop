package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.Book
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class GetBooksUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(search: String?): Result<List<Book>> {
        return libraryRepository.getBooks(search)
    }
}
