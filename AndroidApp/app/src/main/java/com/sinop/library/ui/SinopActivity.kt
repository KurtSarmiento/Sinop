package com.sinop.library.ui

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.appcompat.app.AppCompatDelegate
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.*
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.sinop.library.di.ServiceLocator
import com.sinop.library.ui.screens.bookdetail.BookDetailScreen
import com.sinop.library.ui.screens.catalog.CatalogScreen
import com.sinop.library.ui.screens.home.HomeScreen
import com.sinop.library.ui.screens.login.LoginScreen
import com.sinop.library.ui.screens.mybooks.MyBooksScreen
import com.sinop.library.ui.screens.notifications.NotificationsScreen
import com.sinop.library.ui.screens.profile.ProfileScreen
import com.sinop.library.ui.screens.readinglists.ReadingListsScreen
import com.sinop.library.ui.screens.reservations.ReservationsScreen
import com.sinop.library.ui.theme.SinopTheme

import androidx.activity.enableEdgeToEdge

sealed class BottomNavItem(val route: String, val title: String, val icon: ImageVector) {
    object Home : BottomNavItem("home", "Home", Icons.Default.Home)
    object Catalog : BottomNavItem("catalog", "Catalog", Icons.Default.Search)
    object MyBooks : BottomNavItem("mybooks", "My Books", Icons.Default.Star)
    object Reservations : BottomNavItem("reservations", "Holds", Icons.Default.DateRange)
    object ReadingLists : BottomNavItem("readinglists", "Lists", Icons.AutoMirrored.Filled.List)
    object Profile : BottomNavItem("profile", "Profile", Icons.Default.Person)
}

class SinopActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        enableEdgeToEdge()
        super.onCreate(savedInstanceState)
        AppCompatDelegate.setCompatVectorFromResourcesEnabled(true)
        ServiceLocator.init(applicationContext)

        setContent {
            SinopTheme {
                val navController = rememberNavController()
                val serviceLocator = ServiceLocator.get()
                val navBackStackEntry by navController.currentBackStackEntryAsState()
                val currentRoute = navBackStackEntry?.destination?.route

                val bottomNavItems = listOf(
                    BottomNavItem.Home,
                    BottomNavItem.Catalog,
                    BottomNavItem.MyBooks,
                    BottomNavItem.Reservations,
                    BottomNavItem.ReadingLists,
                    BottomNavItem.Profile
                )

                val showBottomBar = currentRoute != null && currentRoute != "login" && !currentRoute.startsWith("bookdetail")

                Scaffold(
                    bottomBar = {
                        if (showBottomBar) {
                            NavigationBar {
                                bottomNavItems.forEach { item ->
                                    NavigationBarItem(
                                        icon = { Icon(item.icon, contentDescription = item.title) },
                                        label = { Text(item.title, maxLines = 1) },
                                        selected = currentRoute == item.route,
                                        onClick = {
                                            if (currentRoute != item.route) {
                                                navController.navigate(item.route) {
                                                    popUpTo(navController.graph.findStartDestination().id) {
                                                        saveState = true
                                                    }
                                                    launchSingleTop = true
                                                    restoreState = true
                                                }
                                            }
                                        }
                                    )
                                }
                            }
                        }
                    }
                ) { innerPadding ->
                    NavHost(
                        navController = navController,
                        startDestination = "login",
                        modifier = Modifier.padding(bottom = innerPadding.calculateBottomPadding())
                    ) {
                        composable("login") {
                            LoginScreen(
                                onLoginSuccess = {
                                    navController.navigate("home") {
                                        popUpTo("login") { inclusive = true }
                                    }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("home") {
                            HomeScreen(
                                onNavigate = { route ->
                                    navController.navigate(route) {
                                        popUpTo(navController.graph.findStartDestination().id) {
                                            saveState = true
                                        }
                                        launchSingleTop = true
                                        restoreState = true
                                    }
                                },
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") {
                                        popUpTo(0) { inclusive = true }
                                    }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("catalog") {
                            CatalogScreen(
                                onBookClick = { bookId -> navController.navigate("bookdetail/$bookId") },
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("bookdetail/{bookId}") { backStackEntry ->
                            val bookId = backStackEntry.arguments?.getString("bookId")?.toIntOrNull() ?: 0
                            BookDetailScreen(
                                bookId = bookId,
                                onBack = { navController.popBackStack() },
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("mybooks") {
                            MyBooksScreen(
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("reservations") {
                            ReservationsScreen(
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("readinglists") {
                            ReadingListsScreen(
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("notifications") {
                            NotificationsScreen(
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                        composable("profile") {
                            ProfileScreen(
                                onLogout = {
                                    serviceLocator.loginUseCase.logout()
                                    navController.navigate("login") { popUpTo(0) { inclusive = true } }
                                },
                                serviceLocator = serviceLocator
                            )
                        }
                    }
                }
            }
        }
    }
}
