using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class NotificationService
{
    private readonly LibraryDbContext _context;

    public NotificationService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Message = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    public async Task<bool> CreateIfNotExistsAsync(string userId, string message, CancellationToken ct = default)
    {
        var exists = await _context.Notifications
            .AnyAsync(n => n.UserId == userId && n.Message == message && !n.IsRead, ct);
        if (exists) return false;

        await CreateNotificationAsync(new NotificationRequest { UserId = userId, Message = message }, ct);
        return true;
    }

    public async Task<PagedResult<NotificationResponse>> GetNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken ct = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .AsNoTracking();

        if (unreadOnly) query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<NotificationResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);
        if (notification is null) return false;
        notification.IsRead = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);
        foreach (var n in notifications) n.IsRead = true;
        await _context.SaveChangesAsync(ct);
        return notifications.Count;
    }
}
