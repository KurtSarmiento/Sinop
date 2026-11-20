namespace Sinop.Web.DTOs;

public class BookResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public string? Summary { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string[] Authors { get; set; } = Array.Empty<string>();
    public string? CoverUrl { get; set; }
    public int? FirstPublishYear { get; set; }
    public int CopyCount { get; set; }
    public int AvailableCount { get; set; }
}
