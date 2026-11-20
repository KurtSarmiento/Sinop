using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.Services;

namespace Sinop.Web.Jobs;

public class OverdueNotificationJob
{
    private readonly LibraryDbContext _context;
    private readonly NotificationService _notifications;
    private readonly ILogger<OverdueNotificationJob> _logger;

    public OverdueNotificationJob(LibraryDbContext context, NotificationService notifications, ILogger<OverdueNotificationJob> logger)
    {
        _context = context;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("OverdueNotificationJob started at {Time}", DateTime.UtcNow);

        var overdueLoans = await _context.Loans
            .Include(l => l.BookCopy).ThenInclude(c => c.Book)
            .Where(l => l.Status == Sinop.Domain.Enums.LoanStatus.Active && l.DueDate < DateTime.UtcNow)
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} overdue loans", overdueLoans.Count);

        foreach (var loan in overdueLoans)
        {
            // Idempotent: only create a notification if one for this overdue loan doesn't already exist.
            var alreadyNotified = await _context.Notifications
                .AnyAsync(n => n.UserId == loan.UserId
                    && n.Message.Contains($"overdue loan {loan.Id}"), cancellationToken: ct);
            if (alreadyNotified) continue;

            await _notifications.CreateNotificationAsync(new Sinop.Web.DTOs.NotificationRequest
            {
                UserId = loan.UserId,
                Message = $"Your overdue loan {loan.Id} for '{loan.BookCopy!.Book!.Title}' (copy {loan.BookCopy.AccessionNumber}) was due on {loan.DueDate:yyyy-MM-dd}. Please return it."
            }, ct);
        }

        _logger.LogInformation("OverdueNotificationJob completed at {Time}", DateTime.UtcNow);
    }
}
