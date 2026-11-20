package com.sinop.library.domain.usecase

import com.sinop.library.domain.model.Notification
import com.sinop.library.domain.repository.LibraryRepositoryInterface

class GetNotificationsUseCase(private val libraryRepository: LibraryRepositoryInterface) {
    suspend fun invoke(userId: String): Result<List<Notification>> {
        return libraryRepository.getNotifications(userId)
    }
}
