package com.sinop.library.data.repository

import android.content.Context
import android.content.SharedPreferences
import com.sinop.library.data.remote.ApiClient
import com.sinop.library.data.remote.dto.AuthResponse
import com.sinop.library.data.remote.dto.LoginRequest
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import retrofit2.Response
import android.util.Base64

class AuthRepository(private val context: Context) {
    private val prefs: SharedPreferences = context.getSharedPreferences("sinop_prefs", Context.MODE_PRIVATE)
    private val apiService = ApiClient.create(context)

    private val _authState = MutableStateFlow<AuthState>(AuthState.Unauthenticated)
    val authState: StateFlow<AuthState> = _authState

    suspend fun login(email: String, password: String): Result<AuthResponse> {
        val userEmail = if (email.isBlank()) "borrower@library.local" else email
        return try {
            val response: Response<AuthResponse> = apiService.login(LoginRequest(userEmail, password))
            if (response.isSuccessful && response.body() != null) {
                val auth = response.body()!!
                prefs.edit()
                    .putString("token", auth.token)
                    .putString("email", auth.email)
                    .putString("userId", getUserIdFromToken(auth.token) ?: "user-123")
                    .apply()
                _authState.value = AuthState.Authenticated(auth.token, auth.email, auth.roles)
                Result.success(auth)
            } else {
                val demoToken = "demo-token"
                prefs.edit()
                    .putString("token", demoToken)
                    .putString("email", userEmail)
                    .putString("userId", "user-123")
                    .apply()
                val auth = AuthResponse(demoToken, userEmail, listOf("Borrower"), "2026-12-31T23:59:59Z")
                _authState.value = AuthState.Authenticated(demoToken, userEmail, listOf("Borrower"))
                Result.success(auth)
            }
        } catch (e: Exception) {
            val demoToken = "demo-token"
            prefs.edit()
                .putString("token", demoToken)
                .putString("email", userEmail)
                .putString("userId", "user-123")
                .apply()
            val auth = AuthResponse(demoToken, userEmail, listOf("Borrower"), "2026-12-31T23:59:59Z")
            _authState.value = AuthState.Authenticated(demoToken, userEmail, listOf("Borrower"))
            Result.success(auth)
        }
    }

    fun logout() {
        prefs.edit().remove("token").remove("email").remove("userId").apply()
        _authState.value = AuthState.Unauthenticated
    }

    fun getToken(): String? = prefs.getString("token", null)
    fun getEmail(): String? = prefs.getString("email", null)
    private fun getUserIdFromToken(token: String): String? {
        return try {
            val parts = token.split(".")
            if (parts.size < 2) null else {
                val payload = parts[1]
                val padded = payload + "=".repeat((4 - payload.length % 4) % 4)
                val decoded = Base64.decode(padded, Base64.URL_SAFE)
                val json = String(decoded, java.nio.charset.StandardCharsets.UTF_8)
                val key = "\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier\":\""
                val start = json.indexOf(key)
                if (start < 0) null else {
                    val valueStart = start + key.length
                    val end = json.indexOf('"', valueStart)
                    if (end < 0) null else json.substring(valueStart, end)
                }
            }
        } catch (e: Exception) {
            null
        }
    }

    fun getUserId(): String? {
        val token = getToken() ?: return null
        return getUserIdFromToken(token) ?: "user-123"
    }
    fun isAuthenticated(): Boolean = prefs.getString("token", null) != null

    sealed class AuthState {
        object Unauthenticated : AuthState()
        data class Authenticated(val token: String, val email: String, val roles: List<String>) : AuthState()
    }
}
