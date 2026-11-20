using Microsoft.Extensions.Logging;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.Services;

namespace Sinop.Web.Jobs;

public class ReservationExpirationJob
{
    private readonly LibraryDbContext _context;
    private readonly ReservationService _reservations;
    private readonly ILogger<ReservationExpirationJob> _logger;

    public ReservationExpirationJob(LibraryDbContext context, ReservationService reservations, ILogger<ReservationExpirationJob> logger)
    {
        _context = context;
        _reservations = reservations;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("ReservationExpirationJob started at {Time}", DateTime.UtcNow);

        await _reservations.ProcessExpiredReservationsAsync(ct);

        _logger.LogInformation("ReservationExpirationJob completed at {Time}", DateTime.UtcNow);
    }
}
