package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.Book
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class GetBookDetailsUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(bookId: Int): Result<Book> {
        return libraryRepository.getBookDetails(bookId)
    }
}
