namespace Sinop.Web.DTOs;

public class ReadingListResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int ItemCount { get; set; }
    public List<ReadingListItemResponse> Items { get; set; } = new();
}
