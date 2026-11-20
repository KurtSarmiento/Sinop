package com.sinop.library.ui.screens.catalog

import androidx.compose.material3.ExperimentalMaterial3Api
import kotlin.OptIn
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.sinop.library.domain.model.Book
import com.sinop.library.di.ServiceLocator
import com.sinop.library.ui.components.BookCoverImage
import com.sinop.library.ui.components.SinopLogoIcon
import com.sinop.library.ui.components.SinopStatusType
import com.sinop.library.ui.components.StatusPill
import com.sinop.library.ui.theme.SinopTheme
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CatalogScreen(
    onBookClick: (Int) -> Unit,
    onLogout: () -> Unit,
    serviceLocator: ServiceLocator = ServiceLocator.get()
) {
    var searchQuery by remember { mutableStateOf("") }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var books by remember { mutableStateOf<List<Book>>(emptyList()) }
    val scope = rememberCoroutineScope()

    fun loadBooks(query: String? = null) {
        isLoading = true
        error = null
        scope.launch {
            val result = serviceLocator.getBooksUseCase.invoke(query.takeIf { !it.isNullOrBlank() })
            if (result.isSuccess) {
                books = result.getOrThrow()
                error = null
            } else {
                error = result.exceptionOrNull()?.message ?: "Failed to load books"
            }
            isLoading = false
        }
    }

    LaunchedEffect(Unit) {
        loadBooks(null)
    }

    LaunchedEffect(searchQuery) {
        loadBooks(searchQuery)
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
                                "Library Catalog",
                                fontFamily = FontFamily.Serif,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    },
                    actions = {
                        TextButton(onClick = onLogout) { Text("Logout") }
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
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 8.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    OutlinedTextField(
                        value = searchQuery,
                        onValueChange = { searchQuery = it },
                        label = { Text("Search title, author or ISBN") },
                        singleLine = true,
                        modifier = Modifier.weight(1f)
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Button(
                        onClick = { loadBooks(searchQuery) },
                        shape = RoundedCornerShape(8.dp)
                    ) { Text("Search") }
                }

                error?.let { msg ->
                    Text(
                        text = msg,
                        color = MaterialTheme.colorScheme.error,
                        style = MaterialTheme.typography.bodySmall,
                        modifier = Modifier.padding(bottom = 8.dp)
                    )
                }

                if (isLoading && books.isEmpty()) {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.CenterHorizontally))
                } else if (books.isEmpty()) {
                    Box(
                        modifier = Modifier.fillMaxSize(),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = "No books found matching '$searchQuery'",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                } else {
                    LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        contentPadding = PaddingValues(bottom = 16.dp)
                    ) {
                        items(books) { book ->
                            Card(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 6.dp),
                                shape = RoundedCornerShape(10.dp),
                                colors = CardDefaults.cardColors(
                                    containerColor = MaterialTheme.colorScheme.surface
                                ),
                                elevation = CardDefaults.cardElevation(defaultElevation = 1.dp),
                                onClick = { onBookClick(book.id) }
                            ) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(12.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    // Open Library Book Cover
                                    BookCoverImage(
                                        isbn = book.isbn,
                                        title = book.title,
                                        width = 56.dp,
                                        height = 80.dp
                                    )
                                    Spacer(modifier = Modifier.width(14.dp))
                                    Column(
                                        modifier = Modifier.weight(1f)
                                    ) {
                                        Text(
                                            text = book.title,
                                            style = MaterialTheme.typography.titleMedium,
                                            fontFamily = FontFamily.Serif,
                                            fontWeight = FontWeight.Bold
                                        )
                                        if (book.authors.isNotEmpty()) {
                                            Text(
                                                text = "by ${book.authors.joinToString(", ")}",
                                                style = MaterialTheme.typography.bodySmall,
                                                color = MaterialTheme.colorScheme.onSurfaceVariant
                                            )
                                        }
                                        Text(
                                            text = "ISBN: ${book.isbn}",
                                            style = MaterialTheme.typography.labelSmall,
                                            fontFamily = FontFamily.Monospace,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                        Spacer(modifier = Modifier.height(6.dp))
                                        Row(
                                            modifier = Modifier.fillMaxWidth(),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Text(
                                                text = "Copies: ${book.availableCount}/${book.copyCount}",
                                                style = MaterialTheme.typography.labelSmall,
                                                fontFamily = FontFamily.Monospace
                                            )
                                            StatusPill(
                                                type = if (book.availableCount > 0) SinopStatusType.AVAILABLE else SinopStatusType.OVERDUE,
                                                text = if (book.availableCount > 0) "AVAILABLE" else "UNAVAILABLE"
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
