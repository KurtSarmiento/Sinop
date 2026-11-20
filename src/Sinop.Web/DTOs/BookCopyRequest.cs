using Sinop.Domain.Enums;

namespace Sinop.Web.DTOs;

public class BookCopyRequest
{
    public int BookId { get; set; }
    public string? AccessionNumber { get; set; }
    public CopyStatus Status { get; set; } = CopyStatus.Available;
}
