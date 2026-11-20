namespace Sinop.Web.DTOs;

public class BookRequest
{
    public string Title { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public string? Summary { get; set; }
    public int? CategoryId { get; set; }
    public int[] AuthorIds { get; set; } = Array.Empty<int>();
    public string? AuthorNames { get; set; }
    public int CopyCount { get; set; }
}
