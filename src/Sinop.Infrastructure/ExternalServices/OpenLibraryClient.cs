using System.Net;
using System.Text.Json;
using Sinop.Infrastructure.ExternalServices.Models;

namespace Sinop.Infrastructure.ExternalServices;

public class OpenLibraryClient : IOpenLibraryClient
{
    private readonly HttpClient _httpClient;

    public OpenLibraryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OpenLibrarySearchResult?> SearchByTitleAsync(string title, int limit = 10, CancellationToken ct = default)
    {
        var url = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(title)}&limit={limit}&fields=key,title,subtitle,author_name,first_publish_year,isbn,subject,cover_i,cover_edition_key";
        return await GetJsonAsync<OpenLibrarySearchResult>(url, ct);
    }

    public async Task<OpenLibrarySearchResult?> SearchByAuthorAsync(string author, int limit = 10, CancellationToken ct = default)
    {
        var url = $"https://openlibrary.org/search.json?author={Uri.EscapeDataString(author)}&limit={limit}&fields=key,title,subtitle,author_name,first_publish_year,isbn,subject,cover_i,cover_edition_key";
        return await GetJsonAsync<OpenLibrarySearchResult>(url, ct);
    }

    public async Task<OpenLibraryEdition?> GetByIsbnAsync(string isbn, CancellationToken ct = default)
    {
        var url = $"https://openlibrary.org/isbn/{Uri.EscapeDataString(isbn)}.json";
        return await GetJsonAsync<OpenLibraryEdition>(url, ct);
    }

    public async Task<string?> GetCoverUrlAsync(string coverEditionKey, CancellationToken ct = default)
    {
        // Open Library cover API: https://covers.openlibrary.org/b/id/{coverId}-M.jpg
        return $"https://covers.openlibrary.org/b/id/{coverEditionKey}-M.jpg";
    }

    private async Task<T?> GetJsonAsync<T>(string url, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(url, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
