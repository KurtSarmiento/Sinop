using Sinop.Domain.Enums;

namespace Sinop.Web.DTOs;

public class ReservationResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = default!;
    public int? AllocatedCopyId { get; set; }
    public string? AllocatedCopyAccessionNumber { get; set; }
    public string UserId { get; set; } = default!;
    public DateTime ReservedAt { get; set; }
    public DateTime? PickupDeadline { get; set; }
    public ReservationStatus Status { get; set; }
    public int QueuePosition { get; set; }
}
