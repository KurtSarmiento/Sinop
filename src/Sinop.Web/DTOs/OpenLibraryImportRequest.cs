namespace Sinop.Web.DTOs;

public class OpenLibraryImportRequest
{
    public string? Key { get; set; }
    public string Title { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public string? Summary { get; set; }
    public int? CategoryId { get; set; }
    public int[] AuthorIds { get; set; } = Array.Empty<int>();
}
