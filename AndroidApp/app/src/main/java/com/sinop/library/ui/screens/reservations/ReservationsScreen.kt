package com.sinop.library.ui.screens.reservations

import androidx.compose.material3.ExperimentalMaterial3Api
import kotlin.OptIn
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.sinop.library.domain.model.Book
import com.sinop.library.domain.model.Reservation
import com.sinop.library.di.ServiceLocator
import com.sinop.library.ui.components.SinopLogoIcon
import com.sinop.library.ui.theme.SinopTheme
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReservationsScreen(
    onLogout: () -> Unit,
    serviceLocator: ServiceLocator = ServiceLocator.get()
) {
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var reservations by remember { mutableStateOf<List<Reservation>>(emptyList()) }
    var books by remember { mutableStateOf<List<Book>>(emptyList()) }
    var showCreateDialog by remember { mutableStateOf(false) }
    var selectedBookId by remember { mutableStateOf<Int?>(null) }
    var dialogLoading by remember { mutableStateOf(false) }
    val scope = rememberCoroutineScope()

    val userId = serviceLocator.loginUseCase.getUserId() ?: ""

    LaunchedEffect(Unit) {
        if (userId.isNotEmpty()) {
            scope.launch {
                val result = serviceLocator.getReservationsUseCase.invoke(userId)
                if (result.isSuccess) {
                    reservations = result.getOrThrow()
                } else {
                    error = result.exceptionOrNull()?.message ?: "Failed to load reservations"
                }
                isLoading = false
            }
        } else {
            isLoading = false
        }
    }

    LaunchedEffect(Unit) {
        scope.launch {
            val result = serviceLocator.getBooksUseCase.invoke(null)
            if (result.isSuccess) {
                books = result.getOrThrow()
            }
        }
    }

    fun createReservation() {
        val bookId = selectedBookId ?: return
        dialogLoading = true
        scope.launch {
            val result = serviceLocator.createReservationUseCase.invoke(bookId, userId)
            if (result.isSuccess) {
                showCreateDialog = false
                selectedBookId = null
                scope.launch {
                    val r = serviceLocator.getReservationsUseCase.invoke(userId)
                    if (r.isSuccess) reservations = r.getOrThrow()
                }
            } else {
                error = result.exceptionOrNull()?.message ?: "Failed to create reservation"
            }
            dialogLoading = false
        }
    }

    fun cancelReservation(id: Int) {
        scope.launch {
            val result = serviceLocator.cancelReservationUseCase.invoke(id, userId)
            if (result.isSuccess) {
                scope.launch {
                    val r = serviceLocator.getReservationsUseCase.invoke(userId)
                    if (r.isSuccess) reservations = r.getOrThrow()
                }
            } else {
                error = result.exceptionOrNull()?.message ?: "Failed to cancel reservation"
            }
        }
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
                                "Reservations",
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
                    .padding(horizontal = 16.dp)
            ) {
                Spacer(modifier = Modifier.height(8.dp))
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "My Book Holds",
                        style = MaterialTheme.typography.titleLarge,
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.SemiBold
                    )
                    Button(onClick = { showCreateDialog = true }) {
                        Text("Reserve Book")
                    }
                }
                Spacer(modifier = Modifier.height(8.dp))

                error?.let { msg ->
                    Text(text = msg, color = MaterialTheme.colorScheme.error, modifier = Modifier.padding(vertical = 8.dp))
                }

                if (isLoading && reservations.isEmpty()) {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.CenterHorizontally))
                } else if (reservations.isEmpty()) {
                    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                        Text(text = "No active reservations", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
                    }
                } else {
                    LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        contentPadding = PaddingValues(bottom = 16.dp)
                    ) {
                        items(reservations) { r ->
                            Card(
                                modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                            ) {
                                Column(modifier = Modifier.padding(14.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Text(
                                            text = r.bookTitle,
                                            style = MaterialTheme.typography.titleMedium,
                                            fontFamily = FontFamily.Serif,
                                            fontWeight = FontWeight.Bold
                                        )
                                        Text(text = r.status, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.secondary)
                                    }
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(text = "Queue position: ${r.queuePosition + 1}", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                    r.pickupDeadline?.let { deadline ->
                                        Text(text = "Pickup by: $deadline", style = MaterialTheme.typography.bodyMedium, fontFamily = FontFamily.Monospace)
                                    }
                                    Spacer(modifier = Modifier.height(8.dp))
                                    Button(
                                        onClick = { cancelReservation(r.id) },
                                        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.errorContainer)
                                    ) { Text("Cancel Hold") }
                                }
                            }
                        }
                    }
                }
            }

            if (showCreateDialog && books.isNotEmpty()) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Card(
                        modifier = Modifier.width(300.dp).padding(16.dp),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text(text = "Reserve a Book", style = MaterialTheme.typography.titleMedium, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold)
                            Spacer(modifier = Modifier.height(12.dp))
                            var filter by remember { mutableStateOf("") }
                            OutlinedTextField(
                                value = filter,
                                onValueChange = { filter = it },
                                label = { Text("Search books") },
                                singleLine = true,
                                modifier = Modifier.fillMaxWidth()
                            )
                            Spacer(modifier = Modifier.height(8.dp))
                            LazyColumn(
                                modifier = Modifier.height(180.dp).fillMaxWidth()
                            ) {
                                items(books.filter { it.title.contains(filter, ignoreCase = true) }) { book ->
                                    TextButton(
                                        onClick = { selectedBookId = book.id },
                                        modifier = Modifier.fillMaxWidth()
                                    ) {
                                        Text(text = book.title, modifier = Modifier.fillMaxWidth())
                                    }
                                }
                            }
                            Spacer(modifier = Modifier.height(12.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.End
                            ) {
                                TextButton(onClick = { showCreateDialog = false }) { Text("Cancel") }
                                Spacer(modifier = Modifier.width(8.dp))
                                Button(
                                    onClick = { createReservation() },
                                    enabled = selectedBookId != null && !dialogLoading
                                ) {
                                    if (dialogLoading) CircularProgressIndicator(modifier = Modifier.size(16.dp)) else Text("Reserve")
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
