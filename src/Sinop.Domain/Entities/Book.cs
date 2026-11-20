namespace Sinop.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public string? Summary { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
