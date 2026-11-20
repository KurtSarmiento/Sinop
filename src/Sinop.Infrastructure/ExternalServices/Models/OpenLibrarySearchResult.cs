using System.Text.Json.Serialization;

namespace Sinop.Infrastructure.ExternalServices.Models;

public class OpenLibrarySearchResult
{
    public int Start { get; set; }
    public int NumFound { get; set; }
    public bool NumFoundExact { get; set; }
    public List<OpenLibraryWork> Docs { get; set; } = new();
}

public class OpenLibraryWork
{
    public string? Key { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    [JsonPropertyName("author_name")]
    public List<string>? AuthorName { get; set; }
    [JsonPropertyName("author_key")]
    public List<string>? AuthorKey { get; set; }
    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }
    [JsonPropertyName("isbn")]
    public List<string>? ISBN { get; set; }
    [JsonPropertyName("subject")]
    public List<string>? Category { get; set; }
    public System.Text.Json.JsonElement? Ia { get; set; }
    public System.Text.Json.JsonElement? Ica { get; set; }
    public System.Text.Json.JsonElement? Ias { get; set; }
    public string? Publisher { get; set; }
    [JsonPropertyName("publish_place")]
    public List<string>? PublishPlace { get; set; }
    [JsonPropertyName("publish_date")]
    public List<string>? PublishDate { get; set; }
    [JsonPropertyName("cover_edition_key")]
    public string? CoverEditionKey { get; set; }
    [JsonPropertyName("cover_i")]
    public int? CoverId { get; set; }
}

public class OpenLibraryEdition
{
    public string? Key { get; set; }
    public string? Title { get; set; }
    [JsonPropertyName("isbn_10")]
    public List<string>? ISBN10 { get; set; }
    [JsonPropertyName("isbn_13")]
    public List<string>? ISBN13 { get; set; }
    [JsonPropertyName("authors")]
    public List<OpenLibraryEditionAuthor>? Authors { get; set; }
    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }
    public Dictionary<string, List<string>>? Identifiers { get; set; }
    public string? CoverUrl { get; set; }
}

public class OpenLibraryEditionAuthor
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class OpenLibraryAuthor
{
    public string? Key { get; set; }
    public string? Name { get; set; }
    public string? Bio { get; set; }
    public int? BirthDate { get; set; }
    public int? DeathDate { get; set; }
}
