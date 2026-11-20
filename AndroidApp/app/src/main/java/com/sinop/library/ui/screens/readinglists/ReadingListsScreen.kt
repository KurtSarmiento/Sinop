package com.sinop.library.ui.screens.readinglists

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
import com.sinop.library.domain.model.ReadingList
import com.sinop.library.di.ServiceLocator
import com.sinop.library.ui.components.SinopLogoIcon
import com.sinop.library.ui.theme.SinopTheme
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReadingListsScreen(
    onLogout: () -> Unit,
    serviceLocator: ServiceLocator = ServiceLocator.get()
) {
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var lists by remember { mutableStateOf<List<ReadingList>>(emptyList()) }
    var books by remember { mutableStateOf<List<Book>>(emptyList()) }
    var showCreateDialog by remember { mutableStateOf(false) }
    var newListName by remember { mutableStateOf("") }
    var newListDesc by remember { mutableStateOf("") }
    var dialogLoading by remember { mutableStateOf(false) }
    val scope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        scope.launch {
            val result = serviceLocator.getReadingListsUseCase.invoke()
            if (result.isSuccess) {
                lists = result.getOrThrow()
            } else {
                error = result.exceptionOrNull()?.message ?: "Failed to load reading lists"
            }
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

    fun createList() {
        if (newListName.isBlank()) return
        dialogLoading = true
        scope.launch {
            val result = serviceLocator.createReadingListUseCase.invoke(newListName, newListDesc.takeIf { it.isNotBlank() })
            if (result.isSuccess) {
                showCreateDialog = false
                newListName = ""
                newListDesc = ""
                scope.launch {
                    val r = serviceLocator.getReadingListsUseCase.invoke()
                    if (r.isSuccess) lists = r.getOrThrow()
                }
            } else {
                error = result.exceptionOrNull()?.message ?: "Failed to create reading list"
            }
            dialogLoading = false
        }
    }

    fun removeItem(listId: Int, itemId: Int) {
        scope.launch {
            val result = serviceLocator.removeReadingListItemUseCase.invoke(listId, itemId)
            if (result.isSuccess) {
                scope.launch {
                    val r = serviceLocator.getReadingListsUseCase.invoke()
                    if (r.isSuccess) lists = r.getOrThrow()
                }
            } else {
                error = result.exceptionOrNull()?.message ?: "Failed to remove item"
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
                                "Reading Lists",
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
                        text = "My Reading Lists",
                        style = MaterialTheme.typography.titleLarge,
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.SemiBold
                    )
                    Button(onClick = { showCreateDialog = true }) {
                        Text("Create List")
                    }
                }
                Spacer(modifier = Modifier.height(8.dp))

                error?.let { msg ->
                    Text(text = msg, color = MaterialTheme.colorScheme.error, modifier = Modifier.padding(vertical = 8.dp))
                }

                if (isLoading && lists.isEmpty()) {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.CenterHorizontally))
                } else if (lists.isEmpty()) {
                    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                        Text(text = "No reading lists yet", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
                    }
                } else {
                    LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        contentPadding = PaddingValues(bottom = 16.dp)
                    ) {
                        items(lists) { list ->
                            Card(
                                modifier = Modifier.fillMaxWidth().padding(vertical = 6.dp),
                                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                            ) {
                                Column(modifier = Modifier.padding(14.dp)) {
                                    Row(
                                        modifier = Modifier.fillMaxWidth(),
                                        horizontalArrangement = Arrangement.SpaceBetween
                                    ) {
                                        Text(
                                            text = list.name,
                                            style = MaterialTheme.typography.titleMedium,
                                            fontFamily = FontFamily.Serif,
                                            fontWeight = FontWeight.Bold
                                        )
                                        Text(text = "${list.itemCount} items", style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                    }
                                    list.description?.let { desc ->
                                        Spacer(modifier = Modifier.height(2.dp))
                                        Text(text = desc, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                                    }
                                    Spacer(modifier = Modifier.height(8.dp))
                                    list.items.forEach { item ->
                                        Row(
                                            modifier = Modifier.fillMaxWidth().padding(vertical = 2.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Text(text = "• ${item.bookTitle}", style = MaterialTheme.typography.bodyMedium, modifier = Modifier.weight(1f))
                                            TextButton(
                                                onClick = { removeItem(list.id, item.id) }
                                            ) {
                                                Text("Remove", color = MaterialTheme.colorScheme.error)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (showCreateDialog) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Card(
                        modifier = Modifier.width(300.dp).padding(16.dp),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface)
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text(text = "Create Reading List", style = MaterialTheme.typography.titleMedium, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold)
                            Spacer(modifier = Modifier.height(12.dp))
                            OutlinedTextField(
                                value = newListName,
                                onValueChange = { newListName = it },
                                label = { Text("List Name") },
                                singleLine = true,
                                modifier = Modifier.fillMaxWidth()
                            )
                            Spacer(modifier = Modifier.height(8.dp))
                            OutlinedTextField(
                                value = newListDesc,
                                onValueChange = { newListDesc = it },
                                label = { Text("Description (optional)") },
                                singleLine = true,
                                modifier = Modifier.fillMaxWidth()
                            )
                            Spacer(modifier = Modifier.height(16.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.End
                            ) {
                                TextButton(onClick = { showCreateDialog = false }) { Text("Cancel") }
                                Spacer(modifier = Modifier.width(8.dp))
                                Button(
                                    onClick = { createList() },
                                    enabled = newListName.isNotBlank() && !dialogLoading
                                ) {
                                    if (dialogLoading) CircularProgressIndicator(modifier = Modifier.size(16.dp)) else Text("Create")
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
