package com.sinop.library.domain.usecase

import com.sinop.library.data.repository.AuthRepository

class LoginUseCase(private val authRepository: AuthRepository) {
    suspend fun invoke(email: String, password: String): Result<AuthRepository.AuthState.Authenticated> {
        return authRepository.login(email, password).map { auth ->
            AuthRepository.AuthState.Authenticated(auth.token, auth.email, auth.roles)
        }
    }

    fun logout() {
        authRepository.logout()
    }

    fun isAuthenticated(): Boolean = authRepository.isAuthenticated()
    fun getToken(): String? = authRepository.getToken()
    fun getEmail(): String? = authRepository.getEmail()
    fun getUserId(): String? = authRepository.getUserId()
}
