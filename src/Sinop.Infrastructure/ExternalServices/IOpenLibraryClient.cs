using Sinop.Infrastructure.ExternalServices.Models;

namespace Sinop.Infrastructure.ExternalServices;

public interface IOpenLibraryClient
{
    Task<OpenLibrarySearchResult?> SearchByTitleAsync(string title, int limit = 10, CancellationToken ct = default);
    Task<OpenLibrarySearchResult?> SearchByAuthorAsync(string author, int limit = 10, CancellationToken ct = default);
    Task<OpenLibraryEdition?> GetByIsbnAsync(string isbn, CancellationToken ct = default);
    Task<string?> GetCoverUrlAsync(string coverEditionKey, CancellationToken ct = default);
}
