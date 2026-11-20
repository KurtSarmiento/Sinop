namespace Sinop.Web.DTOs;

public class ReadingListRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
