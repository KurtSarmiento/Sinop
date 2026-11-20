namespace Sinop.Web.DTOs;

public class NotificationResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
