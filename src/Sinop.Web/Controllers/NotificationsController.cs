using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize, ct: ct);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(
        string userId,
        CancellationToken ct = default)
    {
        var result = await _notificationService.GetNotificationsAsync(userId, 1, 20, unreadOnly: true, ct: ct);
        return Ok(new { unreadCount = result.TotalCount });
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(
        int id,
        string userId,
        CancellationToken ct = default)
    {
        var marked = await _notificationService.MarkAsReadAsync(id, userId, ct);
        if (!marked) return NotFound(new { message = $"Notification {id} not found." });
        return Ok(new { message = $"Notification {id} marked as read." });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(
        string userId,
        CancellationToken ct = default)
    {
        var count = await _notificationService.MarkAllAsReadAsync(userId, ct);
        return Ok(new { message = $"{count} notifications marked as read." });
    }
}
