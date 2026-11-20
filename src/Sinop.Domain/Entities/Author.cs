namespace Sinop.Domain.Entities;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Bio { get; set; }
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
