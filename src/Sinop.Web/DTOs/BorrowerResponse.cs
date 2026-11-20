namespace Sinop.Web.DTOs;

public class BorrowerResponse
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? FullName { get; set; }
    public string Role { get; set; } = default!;
    public int ActiveLoans { get; set; }
    public int ActiveReservations { get; set; }
}
