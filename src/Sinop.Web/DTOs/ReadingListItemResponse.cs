namespace Sinop.Web.DTOs;

public class ReadingListItemResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = default!;
}
