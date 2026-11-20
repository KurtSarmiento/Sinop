using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinop.Domain.Entities;
using Sinop.Domain.Enums;

namespace Sinop.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
        builder.Property(b => b.ISBN).IsRequired().HasMaxLength(50);
        builder.HasIndex(b => b.ISBN).IsUnique();
        builder.HasOne(b => b.Category)
               .WithMany(c => c.Books)
               .HasForeignKey(b => b.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);
        builder.HasData(
            new Book { Id = 1, Title = "Clean Code", ISBN = "978-0132350884", Summary = "A handbook of agile software craftsmanship.", CategoryId = 2 },
            new Book { Id = 2, Title = "The Pragmatic Programmer", ISBN = "978-0201616224", Summary = "Your journey to mastery.", CategoryId = 2 },
            new Book { Id = 3, Title = "To Kill a Mockingbird", ISBN = "978-0061120084", Summary = "A novel about racial injustice and loss of innocence.", CategoryId = 1 }
        );
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Fictional literature" },
            new Category { Id = 2, Name = "Programming", Description = "Software development" },
            new Category { Id = 3, Name = "Science", Description = "Scientific literature" }
        );
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.HasData(
            new Author { Id = 1, Name = "Robert C. Martin", Bio = "Software engineer and author" },
            new Author { Id = 2, Name = "Martin Fowler", Bio = "Author and speaker on software design" },
            new Author { Id = 3, Name = "Harper Lee", Bio = "American novelist" }
        );
    }
}

public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
{
    public void Configure(EntityTypeBuilder<BookAuthor> builder)
    {
        builder.HasKey(ba => new { ba.BookId, ba.AuthorId });
        builder.HasOne(ba => ba.Book)
               .WithMany(b => b.BookAuthors)
               .HasForeignKey(ba => ba.BookId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ba => ba.Author)
               .WithMany(a => a.BookAuthors)
               .HasForeignKey(ba => ba.AuthorId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 2 },
            new BookAuthor { BookId = 3, AuthorId = 3 }
        );
    }
}

public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.Property(bc => bc.AccessionNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(bc => bc.AccessionNumber).IsUnique();
        builder.HasOne(bc => bc.Book)
               .WithMany(b => b.Copies)
               .HasForeignKey(bc => bc.BookId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(bc => bc.Status).HasConversion<string>();
        builder.HasData(
            new BookCopy { Id = 1, AccessionNumber = "LIB-0001", BookId = 1, Status = CopyStatus.Available },
            new BookCopy { Id = 2, AccessionNumber = "LIB-0002", BookId = 1, Status = CopyStatus.Available },
            new BookCopy { Id = 3, AccessionNumber = "LIB-0003", BookId = 2, Status = CopyStatus.Available },
            new BookCopy { Id = 4, AccessionNumber = "LIB-0004", BookId = 3, Status = CopyStatus.Available }
        );
    }
}

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasOne(l => l.BookCopy)
               .WithMany(c => c.Loans)
               .HasForeignKey(l => l.BookCopyId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.Property(l => l.Status).HasConversion<string>();
        builder.Property(l => l.UserId).IsRequired().HasMaxLength(450);
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasOne(r => r.Book)
               .WithMany(b => b.Reservations)
               .HasForeignKey(r => r.BookId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.AllocatedCopy)
               .WithMany()
               .HasForeignKey(r => r.AllocatedCopyId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.Status).HasConversion<string>();
        builder.Property(r => r.UserId).IsRequired().HasMaxLength(450);
    }
}

public class ReadingListConfiguration : IEntityTypeConfiguration<ReadingList>
{
    public void Configure(EntityTypeBuilder<ReadingList> builder)
    {
        builder.Property(rl => rl.Name).IsRequired().HasMaxLength(100);
        builder.Property(rl => rl.UserId).IsRequired().HasMaxLength(450);
    }
}

public class ReadingListItemConfiguration : IEntityTypeConfiguration<ReadingListItem>
{
    public void Configure(EntityTypeBuilder<ReadingListItem> builder)
    {
        builder.HasKey(rli => rli.Id);
        builder.HasOne(rli => rli.ReadingList)
               .WithMany(rl => rl.Items)
               .HasForeignKey(rli => rli.ReadingListId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(rli => rli.Book)
               .WithMany()
               .HasForeignKey(rli => rli.BookId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Message).IsRequired();
        builder.Property(n => n.UserId).IsRequired().HasMaxLength(450);
    }
}
