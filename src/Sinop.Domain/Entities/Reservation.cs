using Sinop.Domain.Enums;

namespace Sinop.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = default!;
    public int? AllocatedCopyId { get; set; }
    public BookCopy? AllocatedCopy { get; set; }
    public string UserId { get; set; } = default!;
    public DateTime ReservedAt { get; set; }
    public DateTime? PickupDeadline { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Queued;
}
