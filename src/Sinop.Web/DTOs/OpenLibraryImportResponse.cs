namespace Sinop.Web.DTOs;

public class OpenLibraryImportResponse
{
    public int BookId { get; set; }
    public string Title { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public bool Created { get; set; }
}
