namespace Sinop.Domain.Entities;

public class ReadingListItem
{
    public int Id { get; set; }
    public int ReadingListId { get; set; }
    public ReadingList ReadingList { get; set; } = default!;
    public int BookId { get; set; }
    public Book Book { get; set; } = default!;
}
