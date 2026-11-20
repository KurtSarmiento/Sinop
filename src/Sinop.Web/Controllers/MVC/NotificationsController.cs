using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize]
public class NotificationsController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index(string? userId, int page = 1, int pageSize = 20, bool unreadOnly = false)
    {
        if (string.IsNullOrEmpty(userId))
            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize, unreadOnly);
        ViewData["UserId"] = userId;
        ViewData["UnreadOnly"] = unreadOnly;
        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int notificationId, string userId)
    {
        var marked = await _notificationService.MarkAsReadAsync(notificationId, userId);
        if (!marked) return NotFound();
        return RedirectToAction(nameof(Index), new { userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(string userId)
    {
        await _notificationService.MarkAllAsReadAsync(userId);
        return RedirectToAction(nameof(Index), new { userId });
    }
}
