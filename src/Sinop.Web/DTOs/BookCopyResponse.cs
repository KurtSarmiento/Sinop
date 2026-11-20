using Sinop.Domain.Enums;

namespace Sinop.Web.DTOs;

public class BookCopyResponse
{
    public int Id { get; set; }
    public string AccessionNumber { get; set; } = default!;
    public CopyStatus Status { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = default!;
}
