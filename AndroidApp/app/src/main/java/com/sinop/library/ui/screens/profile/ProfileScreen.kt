package com.sinop.library.ui.screens.profile

import androidx.compose.material3.ExperimentalMaterial3Api
import kotlin.OptIn
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.sinop.library.data.repository.AuthRepository
import com.sinop.library.di.ServiceLocator
import com.sinop.library.ui.components.SinopLogoIcon
import com.sinop.library.ui.theme.SinopTheme

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(
    onLogout: () -> Unit,
    serviceLocator: ServiceLocator = ServiceLocator.get()
) {
    val email = serviceLocator.loginUseCase.getEmail() ?: "N/A"
    val authState = serviceLocator.authRepository.authState.value
    val role = when (authState) {
        is AuthRepository.AuthState.Authenticated -> authState.roles.firstOrNull() ?: "Borrower"
        else -> "Borrower"
    }

    SinopTheme {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            SinopLogoIcon(size = 28.dp)
                            Spacer(modifier = Modifier.width(8.dp))
                            Text(
                                "Profile",
                                fontFamily = FontFamily.Serif,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    },
                    actions = {
                        IconButton(onClick = onLogout) {
                            Icon(Icons.AutoMirrored.Filled.ExitToApp, contentDescription = "Logout")
                        }
                    }
                )
            }
        ) { padding ->
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
                    .padding(16.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                Card(
                    modifier = Modifier.fillMaxWidth(0.9f),
                    shape = RoundedCornerShape(12.dp),
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
                    elevation = CardDefaults.cardElevation(defaultElevation = 1.dp)
                ) {
                    Column(
                        modifier = Modifier.padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        SinopLogoIcon(size = 64.dp)
                        Spacer(modifier = Modifier.height(16.dp))
                        Text(
                            text = "My Profile",
                            style = MaterialTheme.typography.headlineSmall,
                            fontFamily = FontFamily.Serif,
                            fontWeight = FontWeight.Bold
                        )
                        Spacer(modifier = Modifier.height(24.dp))

                        Column(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalAlignment = Alignment.Start
                        ) {
                            Text(text = "Email", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
                            Text(text = email, style = MaterialTheme.typography.titleMedium, fontFamily = FontFamily.Monospace)
                            Spacer(modifier = Modifier.height(16.dp))
                            Text(text = "Membership Role", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
                            Text(text = role, style = MaterialTheme.typography.titleMedium, fontFamily = FontFamily.Monospace)
                        }

                        Spacer(modifier = Modifier.height(32.dp))
                        Button(
                            onClick = onLogout,
                            shape = RoundedCornerShape(8.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("Sign Out", fontWeight = FontWeight.Bold)
                        }
                    }
                }
            }
        }
    }
}
