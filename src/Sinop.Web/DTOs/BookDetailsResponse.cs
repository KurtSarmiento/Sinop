namespace Sinop.Web.DTOs;

public class BookDetailsResponse
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
    public List<BookCopyResponse> Copies { get; set; } = new();
}
