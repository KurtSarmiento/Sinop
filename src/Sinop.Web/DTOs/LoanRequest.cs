namespace Sinop.Web.DTOs;

public class LoanRequest
{
    public int BookId { get; set; }
    public string UserId { get; set; } = default!;
}
