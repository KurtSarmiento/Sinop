using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Domain.Enums;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class BookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BookResponse>> GetBooksAsync(int page = 1, int pageSize = 20, string? search = null, int? categoryId = null, string? author = null, bool availableOnly = false, CancellationToken ct = default)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.Copies)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(term) || b.ISBN.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(author))
        {
            var authorTerm = author.ToLower();
            query = query.Where(b => b.BookAuthors.Any(ba => ba.Author != null && ba.Author.Name.ToLower().Contains(authorTerm)));
        }

        if (availableOnly)
            query = query.Where(b => b.Copies.Any(c => c.Status == CopyStatus.Available));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookResponse
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Summary = b.Summary,
                CategoryId = b.CategoryId,
                CategoryName = b.Category != null ? b.Category.Name : null,
                Authors = b.BookAuthors.Where(ba => ba.Author != null).Select(ba => ba.Author!.Name).ToArray(),
                CopyCount = b.Copies.Count,
                AvailableCount = b.Copies.Count(c => c.Status == CopyStatus.Available)
            })
            .ToListAsync(ct);

        return new PagedResult<BookResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<BookDetailsResponse?> GetBookDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.Copies)
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookDetailsResponse
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Summary = b.Summary,
                CategoryId = b.CategoryId,
                CategoryName = b.Category != null ? b.Category.Name : null,
                Authors = b.BookAuthors.Where(ba => ba.Author != null).Select(ba => ba.Author!.Name).ToArray(),
                Copies = b.Copies.Select(c => new BookCopyResponse
                {
                    Id = c.Id,
                    AccessionNumber = c.AccessionNumber,
                    Status = c.Status,
                    BookId = c.BookId,
                    BookTitle = b.Title
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<BookResponse> CreateBookAsync(BookRequest request, CancellationToken ct = default)
    {
        var isDuplicate = await _context.Books.AnyAsync(b => b.ISBN == request.ISBN, ct);
        if (isDuplicate)
            throw new InvalidOperationException($"A book with ISBN {request.ISBN} already exists.");

        var category = request.CategoryId is null ? null :
            await _context.Categories.FindAsync(new object[] { request.CategoryId.Value }, ct)
            ?? throw new InvalidOperationException($"Category {request.CategoryId} not found.");

        var authorIds = request.AuthorIds.ToList();
        foreach (var name in (request.AuthorNames ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower(), ct);
            if (author is null)
            {
                author = new Author { Name = name };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync(ct);
            }
            if (!authorIds.Contains(author.Id)) authorIds.Add(author.Id);
        }

        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Summary = request.Summary,
            Category = category,
            CategoryId = request.CategoryId,
            BookAuthors = authorIds.Select(aid => new BookAuthor { AuthorId = aid }).ToList()
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(ct);

        await CreateCopiesAsync(book.Id, request.CopyCount, ct);
        return await MapToResponseAsync(book, ct);
    }

    public async Task<BookResponse> CreateBookFromOpenLibraryAsync(string title, string isbn, string? summary, IReadOnlyCollection<string> authorNames, int? categoryId = null, int copyCount = 0, CancellationToken ct = default)
    {
        if (await _context.Books.AnyAsync(b => b.ISBN == isbn, ct))
            throw new InvalidOperationException($"A book with ISBN {isbn} already exists.");

        var authorIds = new List<int>();
        foreach (var name in authorNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower(), ct);
            if (author is null)
            {
                author = new Author { Name = name };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync(ct);
            }
            authorIds.Add(author.Id);
        }

        if (categoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId.Value, ct);
            if (!categoryExists)
                throw new InvalidOperationException("The selected category does not exist.");
        }

        var book = new Book
        {
            Title = title,
            ISBN = isbn,
            Summary = string.IsNullOrWhiteSpace(summary) ? "Summary not provided by Open Library." : summary.Trim(),
            CategoryId = categoryId,
            BookAuthors = authorIds.Select(authorId => new BookAuthor { AuthorId = authorId }).ToList()
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(ct);
        await CreateCopiesAsync(book.Id, copyCount, ct);
        return await MapToResponseAsync(book, ct);
    }

    private async Task CreateCopiesAsync(int bookId, int copyCount, CancellationToken ct)
    {
        copyCount = Math.Clamp(copyCount, 0, 1000);
        var prefix = $"LIB-{bookId:D4}";
        var existing = (await _context.BookCopies
            .Where(c => c.AccessionNumber.StartsWith(prefix))
            .Select(c => c.AccessionNumber)
            .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var used = 1;
        for (var index = 1; index <= copyCount; index++)
        {
            string accession;
            do
            {
                accession = $"{prefix}-{used:D2}";
                used++;
            } while (existing.Contains(accession));

            existing.Add(accession);
            _context.BookCopies.Add(new BookCopy
            {
                BookId = bookId,
                AccessionNumber = accession,
                Status = CopyStatus.Available
            });
        }
        if (copyCount > 0)
            await _context.SaveChangesAsync(ct);
    }

    public async Task<BookResponse?> UpdateBookAsync(int id, BookRequest request, CancellationToken ct = default)
    {
        var book = await _context.Books
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) return null;

        var isDuplicate = await _context.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id, ct);
        if (isDuplicate)
            throw new InvalidOperationException($"A book with ISBN {request.ISBN} already exists.");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Summary = request.Summary;
        book.CategoryId = request.CategoryId;

        var authorIds = request.AuthorIds.ToList();
        foreach (var name in (request.AuthorNames ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower(), ct);
            if (author is null)
            {
                author = new Author { Name = name };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync(ct);
            }
            if (!authorIds.Contains(author.Id)) authorIds.Add(author.Id);
        }

        _context.BookAuthors.RemoveRange(book.BookAuthors);
        book.BookAuthors = authorIds.Select(aid => new BookAuthor { AuthorId = aid }).ToList();

        await _context.SaveChangesAsync(ct);
        return await MapToResponseAsync(book, ct);
    }

    public async Task<bool> ArchiveBookAsync(int id, CancellationToken ct = default)
    {
        var book = await _context.Books
            .Include(b => b.Copies).ThenInclude(c => c.Loans)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) return false;

        var hasActiveLoan = book.Copies.Any(c => c.Loans.Any(l => l.Status == LoanStatus.Active));
        var hasActiveReservation = book.Reservations.Any(r => r.Status == ReservationStatus.Queued);
        if (hasActiveLoan || hasActiveReservation)
            throw new InvalidOperationException("Cannot archive a book with active loans or active reservations.");

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsDuplicateIsbnAsync(string isbn, CancellationToken ct = default, int? excludeId = null)
    {
        var query = _context.Books.Where(b => b.ISBN == isbn);
        if (excludeId is not null) query = query.Where(b => b.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    private async Task<BookResponse> MapToResponseAsync(Book book, CancellationToken ct)
    {
        var entry = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.Copies)
            .AsNoTracking()
            .FirstAsync(b => b.Id == book.Id, ct);

        return new BookResponse
        {
            Id = entry.Id,
            Title = entry.Title,
            ISBN = entry.ISBN,
            Summary = entry.Summary,
            CategoryId = entry.CategoryId,
            CategoryName = entry.Category?.Name,
            Authors = entry.BookAuthors.Where(ba => ba.Author != null).Select(ba => ba.Author!.Name).ToArray(),
            CopyCount = entry.Copies.Count,
            AvailableCount = entry.Copies.Count(c => c.Status == CopyStatus.Available)
        };
    }
}
