using Sinop.Domain.Enums;

namespace Sinop.Domain.Entities;

public class BookCopy
{
    public int Id { get; set; }
    public string AccessionNumber { get; set; } = default!;
    public CopyStatus Status { get; set; } = CopyStatus.Available;
    public int BookId { get; set; }
    public Book Book { get; set; } = default!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
