using Microsoft.EntityFrameworkCore;
using Sinop.Infrastructure.ExternalServices.Models;
using Sinop.Infrastructure.Persistence;

namespace Sinop.Infrastructure.ExternalServices;

public class OpenLibraryService : IOpenLibraryService
{
    private readonly IOpenLibraryClient _client;
    private readonly LibraryDbContext _context;

    public OpenLibraryService(IOpenLibraryClient client, LibraryDbContext context)
    {
        _client = client;
        _context = context;
    }

    public async Task<OpenLibrarySearchResult?> SearchByTitleAsync(string title, int limit = 10, CancellationToken ct = default)
    {
        return await _client.SearchByTitleAsync(title, limit, ct);
    }

    public async Task<OpenLibrarySearchResult?> SearchByAuthorAsync(string author, int limit = 10, CancellationToken ct = default)
    {
        return await _client.SearchByAuthorAsync(author, limit, ct);
    }

    public async Task<OpenLibraryEdition?> GetByIsbnAsync(string isbn, CancellationToken ct = default)
    {
        return await _client.GetByIsbnAsync(isbn, ct);
    }

    public async Task<string?> GetCoverUrlAsync(string coverEditionKey, CancellationToken ct = default)
    {
        return await _client.GetCoverUrlAsync(coverEditionKey, ct);
    }

    public async Task<bool> IsDuplicateIsbnAsync(string isbn, CancellationToken ct = default)
    {
        return await _context.Books.AnyAsync(b => b.ISBN == isbn, ct);
    }
}
