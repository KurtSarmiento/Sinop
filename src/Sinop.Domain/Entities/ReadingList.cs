namespace Sinop.Domain.Entities;

public class ReadingList
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public ICollection<ReadingListItem> Items { get; set; } = new List<ReadingListItem>();
}
