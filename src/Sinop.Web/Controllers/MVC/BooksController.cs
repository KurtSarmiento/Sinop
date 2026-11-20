using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Infrastructure.ExternalServices;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize]
public class BooksController : Controller
{
    private readonly BookService _bookService;
    private readonly IOpenLibraryService _openLibraryService;

    public BooksController(BookService bookService, IOpenLibraryService openLibraryService)
    {
        _bookService = bookService;
        _openLibraryService = openLibraryService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, int? categoryId = null, string? author = null, bool availableOnly = false, int? publicationYear = null, CancellationToken ct = default)
    {
        var result = await _bookService.GetBooksAsync(page, pageSize, search, categoryId, author, availableOnly, ct);

        foreach (var book in result.Items)
        {
            book.CoverUrl = $"https://covers.openlibrary.org/b/isbn/{Uri.EscapeDataString(book.ISBN)}-M.jpg";

            var edition = await _openLibraryService.GetByIsbnAsync(book.ISBN, ct);
            if (edition?.PublishDate is not null)
            {
                var yearMatch = System.Text.RegularExpressions.Regex.Match(edition.PublishDate, @"\d{4}");
                if (yearMatch.Success && int.TryParse(yearMatch.Value, out var year))
                    book.FirstPublishYear = year;
            }
        }

        if (publicationYear.HasValue)
        {
            result.Items = result.Items.Where(book => book.FirstPublishYear == publicationYear.Value).ToList();
            result.TotalCount = result.Items.Count;
        }

        ViewData["Search"] = search;
        ViewData["CategoryId"] = categoryId;
        ViewData["Author"] = author;
        ViewData["AvailableOnly"] = availableOnly;
        ViewData["PublicationYear"] = publicationYear;
        ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);
        return View(result);
    }

    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> ExternalSearch(string? search, string? author = null, int? publicationYear = null, int page = 1, int pageSize = 12, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 12;
        ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);

        if (string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(author))
        {
            ViewData["Search"] = "";
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = 1;
            return View(new PagedResult<OpenLibraryBookPreview>());
        }

        var result = !string.IsNullOrWhiteSpace(author)
            ? await _openLibraryService.SearchByAuthorAsync(author, 100, ct)
            : await _openLibraryService.SearchByTitleAsync(search ?? string.Empty, 100, ct);

        var previews = result?.Docs
            .Where(work => !publicationYear.HasValue || work.FirstPublishYear == publicationYear.Value)
            .Where(work => string.IsNullOrWhiteSpace(search) || work.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
            .Select(work => new OpenLibraryBookPreview
        {
            Key = work.Key,
            Title = work.Title,
            Subtitle = work.Subtitle,
            Authors = work.AuthorName?.ToArray() ?? Array.Empty<string>(),
            FirstPublishYear = work.FirstPublishYear,
            ISBNs = work.ISBN?.ToArray() ?? Array.Empty<string>(),
            Categories = work.Category?.ToArray() ?? Array.Empty<string>(),
            CoverUrl = work.ISBN?.FirstOrDefault() is string isbn
                ? $"https://covers.openlibrary.org/b/isbn/{Uri.EscapeDataString(isbn)}-M.jpg"
                : null
        }).ToList() ?? new List<OpenLibraryBookPreview>();

        foreach (var preview in previews)
        {
            var isbn = preview.ISBNs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(isbn))
                preview.IsDuplicate = await _openLibraryService.IsDuplicateIsbnAsync(isbn, ct);
        }

        var totalCount = previews.Count;
        var pagedItems = previews
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewData["Search"] = search;
        ViewData["Author"] = author;
        ViewData["PublicationYear"] = publicationYear;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalPages"] = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        return View(new PagedResult<OpenLibraryBookPreview>
        {
            Items = pagedItems,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExternal(string title, string isbn, string? authors, string? summary, int? categoryId, int copyCount, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(isbn))
            return RedirectToAction(nameof(ExternalSearch));

        try
        {
            var authorNames = (authors ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var book = await _bookService.CreateBookFromOpenLibraryAsync(title, isbn, summary, authorNames, categoryId, copyCount, ct);

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(ExternalSearch));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var book = await _bookService.GetBookDetailsAsync(id);
        if (book is null) return NotFound();

        book.CoverUrl = $"https://covers.openlibrary.org/b/isbn/{Uri.EscapeDataString(book.ISBN)}-M.jpg";
        var edition = await _openLibraryService.GetByIsbnAsync(book.ISBN);
        if (edition?.PublishDate is not null)
        {
            var yearMatch = System.Text.RegularExpressions.Regex.Match(edition.PublishDate, @"\d{4}");
            if (yearMatch.Success && int.TryParse(yearMatch.Value, out var year))
                book.FirstPublishYear = year;
        }

        return View(book);
    }

    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);
        return View(new BookRequest());
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);
            return View(request);
        }
        try
        {
            var book = await _bookService.CreateBookAsync(request);
            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        catch (InvalidOperationException ex)
        {
            ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _bookService.GetBookDetailsAsync(id);
        if (book is null) return NotFound();
        ViewData["Categories"] = await _bookService.GetCategoriesAsync();
        return View(new BookRequest
        {
            Title = book.Title,
            ISBN = book.ISBN,
            Summary = book.Summary,
            CategoryId = book.CategoryId,
            AuthorNames = string.Join(", ", book.Authors),
            AuthorIds = Array.Empty<int>(),
            CopyCount = book.Copies.Count
        });
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);
            return View(request);
        }
        try
        {
            var book = await _bookService.UpdateBookAsync(id, request);
            if (book is null) return NotFound();
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException ex)
        {
            ViewData["Categories"] = await _bookService.GetCategoriesAsync(ct);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            var archived = await _bookService.ArchiveBookAsync(id);
            if (!archived) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
