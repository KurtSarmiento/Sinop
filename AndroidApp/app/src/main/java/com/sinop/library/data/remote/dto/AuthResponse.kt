package com.sinop.library.data.remote.dto

data class AuthResponse(
    val token: String,
    val email: String,
    val roles: List<String>,
    val expiresAt: String
)
