namespace Sinop.Web.DTOs;

public class ReservationRequest
{
    public int BookId { get; set; }
    public string UserId { get; set; } = default!;
}
