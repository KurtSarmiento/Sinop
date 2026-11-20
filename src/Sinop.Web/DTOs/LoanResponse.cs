using Sinop.Domain.Enums;

namespace Sinop.Web.DTOs;

public class LoanResponse
{
    public int Id { get; set; }
    public int BookCopyId { get; set; }
    public string AccessionNumber { get; set; } = default!;
    public string BookTitle { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string UserEmail { get; set; } = default!;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public LoanStatus Status { get; set; }
    public bool IsOverdue => Status == LoanStatus.Active && DateTime.UtcNow > DueDate;
}
