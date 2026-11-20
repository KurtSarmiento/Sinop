using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Domain.Enums;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class ReservationService
{
    private readonly LibraryDbContext _context;
    private readonly NotificationService _notifications;
    private readonly LoanService _loans;

    public ReservationService(LibraryDbContext context, NotificationService notifications, LoanService loans)
    {
        _context = context;
        _notifications = notifications;
        _loans = loans;
    }

    public async Task<PagedResult<ReservationResponse>> GetReservationsAsync(
        int page = 1, int pageSize = 20, string? userId = null, int? bookId = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.AllocatedCopy)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(userId)) query = query.Where(r => r.UserId == userId);
        if (bookId is not null) query = query.Where(r => r.BookId == bookId.Value);

        var total = await query.CountAsync(ct);
        var entities = await query
            .OrderBy(r => r.ReservedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<ReservationResponse>();
        var position = 1;
        foreach (var r in entities)
        {
            var queuePos = r.Status == ReservationStatus.Queued ? position++ : 0;
            items.Add(await MapToResponseAsync(r, queuePos, ct));
        }

        return new PagedResult<ReservationResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<ReservationResponse?> GetReservationAsync(int id, CancellationToken ct = default)
    {
        var r = await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.AllocatedCopy)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (r is null) return null;
        var position = r.Status == ReservationStatus.Queued
            ? await _context.Reservations
                .CountAsync(x => x.BookId == r.BookId && x.Status == ReservationStatus.Queued && x.ReservedAt < r.ReservedAt, ct) + 1
            : 0;
        return await MapToResponseAsync(r, position, ct);
    }

    public async Task<ReservationResponse> CreateReservationAsync(ReservationRequest request, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                ?? throw new InvalidOperationException($"User {request.UserId} not found.");

            var book = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.BookId, ct)
                ?? throw new InvalidOperationException($"Book {request.BookId} not found.");

            // Prevent duplicate active (Queued) reservations for the same borrower + title.
            var duplicate = await _context.Reservations
                .AnyAsync(r => r.UserId == request.UserId && r.BookId == request.BookId
                    && r.Status == ReservationStatus.Queued, ct);
            if (duplicate)
                throw new InvalidOperationException($"User already has an active reservation for '{book.Title}'.");

            var reservation = new Reservation
            {
                BookId = request.BookId,
                UserId = request.UserId,
                ReservedAt = DateTime.UtcNow,
                PickupDeadline = DateTime.UtcNow.AddDays(LibraryRules.ReservationPickupPeriodDays),
                Status = ReservationStatus.Queued
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync(ct);

            // Deterministic queue position: count of earlier queued reservations for the same book + 1.
            var position = await _context.Reservations
                .CountAsync(r => r.BookId == request.BookId && r.Status == ReservationStatus.Queued
                    && r.ReservedAt < reservation.ReservedAt, ct) + 1;

            await transaction.CommitAsync(ct);

            var created = await GetReservationAsync(reservation.Id, ct);
            created!.QueuePosition = position;
            return created;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> CancelReservationAsync(int id, string userId, CancellationToken ct = default)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return false;
        if (reservation.UserId != userId) return false;
        if (reservation.Status != ReservationStatus.Queued && reservation.Status != ReservationStatus.ReadyForPickup)
            return false;

        reservation.Status = ReservationStatus.Cancelled;

        // Release allocated copy back to Available.
        if (reservation.AllocatedCopyId is not null)
        {
            var copy = await _context.BookCopies.FirstOrDefaultAsync(c => c.Id == reservation.AllocatedCopyId.Value, ct);
            if (copy is not null && copy.Status == CopyStatus.Reserved)
                copy.Status = CopyStatus.Available;
            reservation.AllocatedCopyId = null;
        }

        await _context.SaveChangesAsync(ct);

        // Allocate the released copy to the next queued borrower.
        await AllocateNextAsync(reservation.BookId, ct);
        return true;
    }

    public async Task<bool> ExpireReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return false;
        if (reservation.Status != ReservationStatus.Queued) return false;
        if (reservation.PickupDeadline == null || reservation.PickupDeadline.Value >= DateTime.UtcNow) return false;

        reservation.Status = ReservationStatus.Expired;

        if (reservation.AllocatedCopyId is not null)
        {
            var copy = await _context.BookCopies.FirstOrDefaultAsync(c => c.Id == reservation.AllocatedCopyId.Value, ct);
            if (copy is not null && copy.Status == CopyStatus.Reserved)
                copy.Status = CopyStatus.Available;
            reservation.AllocatedCopyId = null;
        }

        await _context.SaveChangesAsync(ct);

        // Notify the borrower that the reservation expired.
        await _notifications.CreateIfNotExistsAsync(reservation.UserId,
            $"Your reservation for reservation {id} has expired and the copy was released.", ct);

        // Allocate to the next queued borrower.
        await AllocateNextAsync(reservation.BookId, ct);
        return true;
    }

    public async Task ProcessExpiredReservationsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expired = await _context.Reservations
            .Where(r => r.PickupDeadline != null && r.PickupDeadline.Value < now)
            .ToListAsync(ct);

        foreach (var r in expired)
        {
            if (r.Status == ReservationStatus.Queued)
                await ExpireReservationAsync(r.Id, ct);
        }
    }

    // Called when a copy is returned: allocate it to the next queued borrower (first-come, first-served).
    // Returns true if a copy was allocated to a waiting borrower; false if no copy or no queue.
    public async Task<bool> AllocateReturnedCopyAsync(int bookId, int copyId, CancellationToken ct = default)
    {
        var copy = await _context.BookCopies.FirstOrDefaultAsync(c => c.Id == copyId, ct);
        if (copy is null || copy.Status != CopyStatus.Available) return false;

        var next = await _context.Reservations
            .Include(r => r.Book)
            .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Queued)
            .OrderBy(r => r.ReservedAt)
            .FirstOrDefaultAsync(ct);

        if (next is null) return false; // No waiting borrower; copy stays Available.

        copy.Status = CopyStatus.Reserved;
        next.Status = ReservationStatus.ReadyForPickup;
        next.AllocatedCopyId = copy.Id;
        next.PickupDeadline = DateTime.UtcNow.AddDays(LibraryRules.ReservationPickupPeriodDays);

        await _context.SaveChangesAsync(ct);

        await _notifications.CreateIfNotExistsAsync(next.UserId,
            $"Your reserved book '{next.Book!.Title}' is ready for pickup (copy {copy.AccessionNumber}).", ct);

        return true;
    }

    private async Task AllocateNextAsync(int bookId, CancellationToken ct)
    {
        var next = await _context.Reservations
            .Include(r => r.Book)
            .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Queued)
            .OrderBy(r => r.ReservedAt)
            .FirstOrDefaultAsync(ct);

        if (next is null) return;

        // Find an Available copy for this book.
        var copy = await _context.BookCopies
            .Where(c => c.BookId == bookId && c.Status == CopyStatus.Available)
            .OrderBy(c => c.AccessionNumber)
            .FirstOrDefaultAsync(ct);

        if (copy is null) return; // No copy available; reservation stays Queued.

        copy.Status = CopyStatus.Reserved;
        next.Status = ReservationStatus.ReadyForPickup;
        next.AllocatedCopyId = copy.Id;
        next.PickupDeadline = DateTime.UtcNow.AddDays(LibraryRules.ReservationPickupPeriodDays);

        await _context.SaveChangesAsync(ct);

        await _notifications.CreateIfNotExistsAsync(next.UserId,
            $"Your reserved book '{next.Book!.Title}' is ready for pickup (copy {copy.AccessionNumber}).", ct);
    }

    private async Task<ReservationResponse> MapToResponseAsync(Reservation r, int queuePosition, CancellationToken ct)
    {
        var bookTitle = r.Book?.Title ?? "";
        var accession = r.AllocatedCopy?.AccessionNumber;
        return new ReservationResponse
        {
            Id = r.Id,
            BookId = r.BookId,
            BookTitle = bookTitle,
            AllocatedCopyId = r.AllocatedCopyId,
            AllocatedCopyAccessionNumber = accession,
            UserId = r.UserId,
            ReservedAt = r.ReservedAt,
            PickupDeadline = r.PickupDeadline,
            Status = r.Status,
            QueuePosition = queuePosition
        };
    }
}
