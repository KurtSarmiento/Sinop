using Sinop.Domain.Enums;

namespace Sinop.Domain.Entities;

public class Loan
{
    public int Id { get; set; }
    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;
}
