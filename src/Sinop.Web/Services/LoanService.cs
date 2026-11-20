using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Domain.Enums;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class LoanService
{
    private readonly LibraryDbContext _context;
    private readonly NotificationService _notifications;

    public LoanService(LibraryDbContext context, NotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<LoanResponse> CheckoutAsync(LoanRequest request, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // 1. Validate the borrower exists.
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                ?? throw new InvalidOperationException($"User {request.UserId} not found.");

            // 2. Validate book exists.
            var book = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.BookId, ct)
                ?? throw new InvalidOperationException($"Book {request.BookId} not found.");

            // 3. Prevent duplicate active loans for the same borrower and title.
            var hasActiveLoanForTitle = await _context.Loans
                .Include(l => l.BookCopy).ThenInclude(c => c.Book)
                .AnyAsync(l => l.UserId == request.UserId
                    && l.BookCopy.BookId == request.BookId
                    && l.Status == LoanStatus.Active, ct);
            if (hasActiveLoanForTitle)
                throw new InvalidOperationException($"User already has an active loan for '{book.Title}'.");

            // 4. Validate borrowing limit (max 3 simultaneous active loans).
            var activeLoanCount = await _context.Loans
                .CountAsync(l => l.UserId == request.UserId && l.Status == LoanStatus.Active, ct);
            if (activeLoanCount >= LibraryRules.MaxSimultaneousActiveLoans)
                throw new InvalidOperationException($"Borrowing limit reached: maximum {LibraryRules.MaxSimultaneousActiveLoans} simultaneous active loans.");

            // 5. Find an available copy with row-level locking (UPDLOCK, READPAST) to prevent
            //    concurrent borrowing from allocating the same copy twice.
            var availableCopy = await _context.BookCopies
                .Include(c => c.Book)
                .Where(c => c.BookId == request.BookId && c.Status == CopyStatus.Available)
                .OrderBy(c => c.AccessionNumber)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (availableCopy is null)
                throw new InvalidOperationException($"No available copies of '{book.Title}'.");

            // Re-fetch the copy inside the transaction with an update lock to re-check status
            // under concurrency (re-check eligibility and copy status in the transaction).
            var copy = await _context.BookCopies
                .Where(c => c.Id == availableCopy.Id)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException($"Copy {availableCopy.Id} no longer exists.");

            if (copy.Status != CopyStatus.Available)
                throw new InvalidOperationException($"Copy {copy.AccessionNumber} is no longer available (status: {copy.Status}).");

            // 6. Allocate the copy: mark it Borrowed and create the loan.
            copy.Status = CopyStatus.Borrowed;
            var loan = new Loan
            {
                BookCopyId = copy.Id,
                UserId = request.UserId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(LibraryRules.LoanDurationDays),
                ReturnDate = null,
                Status = LoanStatus.Active
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return await MapToResponseAsync(loan, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<LoanResponse> ReturnAsync(int loanId, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                .FirstOrDefaultAsync(l => l.Id == loanId, ct)
                ?? throw new InvalidOperationException($"Loan {loanId} not found.");

            // Prevent duplicate returns.
            if (loan.Status != LoanStatus.Active)
                throw new InvalidOperationException($"Loan {loanId} is already {loan.Status} and cannot be returned again.");

            // Record return and release the copy.
            loan.ReturnDate = DateTime.UtcNow;
            loan.Status = LoanStatus.Returned;

            var copy = await _context.BookCopies
                .Where(c => c.Id == loan.BookCopyId)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException($"Copy {loan.BookCopyId} not found.");

            copy.Status = CopyStatus.Available;

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return await MapToResponseAsync(loan, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<PagedResult<LoanResponse>> GetActiveLoansAsync(int page = 1, int pageSize = 20, string? userId = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Loans
            .Include(l => l.BookCopy).ThenInclude(c => c.Book)
            .Where(l => l.Status == LoanStatus.Active)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);

        var total = await query.CountAsync(ct);
        var entities = await query
            .OrderBy(l => l.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities.Select(MapToResponseDto).ToList();
        return new PagedResult<LoanResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<PagedResult<LoanResponse>> GetLoanHistoryAsync(string userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Loans
            .Include(l => l.BookCopy).ThenInclude(c => c.Book)
            .Where(l => l.UserId == userId)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var entities = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities.Select(MapToResponseDto).ToList();
        return new PagedResult<LoanResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    private static LoanResponse MapToResponseDto(Loan l)
    {
        return new LoanResponse
        {
            Id = l.Id,
            BookCopyId = l.BookCopyId,
            AccessionNumber = l.BookCopy!.AccessionNumber,
            BookTitle = l.BookCopy!.Book!.Title,
            UserId = l.UserId,
            UserEmail = "",
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            Status = l.Status
        };
    }

    public async Task<bool> ValidateBorrowerEligibilityAsync(string userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return false;

        var activeLoanCount = await _context.Loans
            .CountAsync(l => l.UserId == userId && l.Status == LoanStatus.Active, ct);
        return activeLoanCount < LibraryRules.MaxSimultaneousActiveLoans;
    }

    public async Task<int> GetOverdueCountAsync(CancellationToken ct = default)
    {
        return await _context.Loans
            .CountAsync(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow, ct);
    }

    private async Task<LoanResponse> MapToResponseAsync(Loan loan, CancellationToken ct)
    {
        var entry = await _context.Loans
            .Include(l => l.BookCopy).ThenInclude(c => c.Book)
            .AsNoTracking()
            .FirstAsync(l => l.Id == loan.Id, ct);

        var user = await _context.Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == entry.UserId, ct);

        return new LoanResponse
        {
            Id = entry.Id,
            BookCopyId = entry.BookCopyId,
            AccessionNumber = entry.BookCopy!.AccessionNumber,
            BookTitle = entry.BookCopy.Book!.Title,
            UserId = entry.UserId,
            UserEmail = user.Email!,
            LoanDate = entry.LoanDate,
            DueDate = entry.DueDate,
            ReturnDate = entry.ReturnDate,
            Status = entry.Status
        };
    }
}
