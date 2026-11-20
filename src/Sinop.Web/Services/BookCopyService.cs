using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Domain.Enums;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class BookCopyService
{
    private readonly LibraryDbContext _context;

    public BookCopyService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<BookCopyResponse?> GetCopyAsync(int id, CancellationToken ct = default)
    {
        return await _context.BookCopies
            .Include(c => c.Book)
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new BookCopyResponse
            {
                Id = c.Id,
                AccessionNumber = c.AccessionNumber,
                Status = c.Status,
                BookId = c.BookId,
                BookTitle = c.Book!.Title
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<BookCopyResponse>> GetCopiesAsync(int page = 1, int pageSize = 20, int? bookId = null, CopyStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.BookCopies
            .Include(c => c.Book)
            .AsNoTracking();

        if (bookId is not null) query = query.Where(c => c.BookId == bookId.Value);
        if (status is not null) query = query.Where(c => c.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.AccessionNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new BookCopyResponse
            {
                Id = c.Id,
                AccessionNumber = c.AccessionNumber,
                Status = c.Status,
                BookId = c.BookId,
                BookTitle = c.Book!.Title
            })
            .ToListAsync(ct);

        return new PagedResult<BookCopyResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<BookCopyResponse> RegisterCopyAsync(BookCopyRequest request, CancellationToken ct = default)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == request.BookId, ct);
        if (!bookExists)
            throw new InvalidOperationException($"Book {request.BookId} not found.");

        var accession = string.IsNullOrWhiteSpace(request.AccessionNumber)
            ? await GenerateAccessionNumberAsync(request.BookId, ct)
            : request.AccessionNumber;

        var accessionExists = await _context.BookCopies.AnyAsync(c => c.AccessionNumber == accession, ct);
        if (accessionExists)
            throw new InvalidOperationException($"Accession number {accession} already exists.");

        var copy = new BookCopy
        {
            AccessionNumber = accession,
            Status = request.Status,
            BookId = request.BookId
        };

        _context.BookCopies.Add(copy);
        await _context.SaveChangesAsync(ct);

        return new BookCopyResponse
        {
            Id = copy.Id,
            AccessionNumber = copy.AccessionNumber,
            Status = copy.Status,
            BookId = copy.BookId,
            BookTitle = (await _context.Books.AsNoTracking().Where(b => b.Id == copy.BookId).Select(b => b.Title).FirstAsync(ct))!
        };
    }

    public async Task<bool> UpdateCopyStatusAsync(int id, CopyStatus status, CancellationToken ct = default)
    {
        var copy = await _context.BookCopies.FindAsync(new object[] { id }, ct);
        if (copy is null) return false;

        if (!IsValidTransition(copy.Status, status))
            throw new InvalidOperationException($"Invalid copy status transition from {copy.Status} to {status}.");

        copy.Status = status;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static bool IsValidTransition(CopyStatus from, CopyStatus to)
    {
        if (from == to) return true;
        return from switch
        {
            CopyStatus.Available => to is CopyStatus.Borrowed or CopyStatus.Reserved or CopyStatus.Maintenance or CopyStatus.Lost,
            CopyStatus.Borrowed => to is CopyStatus.Available or CopyStatus.Maintenance or CopyStatus.Lost,
            CopyStatus.Reserved => to is CopyStatus.Available or CopyStatus.Borrowed or CopyStatus.Maintenance or CopyStatus.Lost,
            CopyStatus.Maintenance => to is CopyStatus.Available or CopyStatus.Lost,
            CopyStatus.Lost => to is CopyStatus.Available or CopyStatus.Maintenance,
            _ => false
        };
    }

    public async Task<string> GenerateAccessionNumberAsync(int bookId, CancellationToken ct = default)
    {
        var book = await _context.Books.AsNoTracking().Where(b => b.Id == bookId).Select(b => b.Title).FirstOrDefaultAsync(ct);
        if (book is null)
            throw new InvalidOperationException($"Book {bookId} not found.");

        var prefix = $"LIB-{bookId:D4}";
        var existing = await _context.BookCopies
            .Where(c => c.AccessionNumber.StartsWith(prefix))
            .Select(c => c.AccessionNumber)
            .ToListAsync(ct);

        var next = 1;
        while (existing.Contains($"{prefix}-{next:D2}")) next++;

        return $"{prefix}-{next:D2}";
    }

    public async Task<int> GetAvailableCountAsync(int bookId, CancellationToken ct = default)
    {
        return await _context.BookCopies
            .CountAsync(c => c.BookId == bookId && c.Status == CopyStatus.Available, ct);
    }
}
