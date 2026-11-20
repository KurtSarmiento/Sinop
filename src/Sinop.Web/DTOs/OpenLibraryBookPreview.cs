namespace Sinop.Web.DTOs;

public class OpenLibraryBookPreview
{
    public string? Key { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string[] Authors { get; set; } = Array.Empty<string>();
    public int? FirstPublishYear { get; set; }
    public string[] ISBNs { get; set; } = Array.Empty<string>();
    public string[] Categories { get; set; } = Array.Empty<string>();
    public string? CoverUrl { get; set; }
    public bool IsDuplicate { get; set; }
}
