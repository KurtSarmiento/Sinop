using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Infrastructure.ExternalServices;
using Sinop.Infrastructure.ExternalServices.Models;
using Sinop.Web.DTOs;
using Sinop.Web.Services;
using System.Linq;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Librarian,Administrator")]
public class OpenLibraryController : ControllerBase
{
    private readonly IOpenLibraryService _openLibraryService;
    private readonly BookService _bookService;

    public OpenLibraryController(IOpenLibraryService openLibraryService, BookService bookService)
    {
        _openLibraryService = openLibraryService;
        _bookService = bookService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string title, int limit = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Title is required." });

        var result = await _openLibraryService.SearchByTitleAsync(title, limit, ct);
        if (result is null)
            return NotFound(new { message = "No results found." });

        var previews = result.Docs.Select(MapToPreview).ToList();
        foreach (var preview in previews)
        {
            if (!string.IsNullOrEmpty(preview.ISBNs?.FirstOrDefault()))
                preview.IsDuplicate = await _openLibraryService.IsDuplicateIsbnAsync(preview.ISBNs.First(), ct);
        }

        return Ok(new { total = result.NumFound, results = previews });
    }

    [HttpGet("search-by-author")]
    public async Task<IActionResult> SearchByAuthor(string author, int limit = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(author))
            return BadRequest(new { message = "Author is required." });

        var result = await _openLibraryService.SearchByAuthorAsync(author, limit, ct);
        if (result is null)
            return NotFound(new { message = "No results found." });

        var previews = result.Docs.Select(MapToPreview).ToList();
        foreach (var preview in previews)
        {
            if (!string.IsNullOrEmpty(preview.ISBNs?.FirstOrDefault()))
                preview.IsDuplicate = await _openLibraryService.IsDuplicateIsbnAsync(preview.ISBNs.First(), ct);
        }

        return Ok(new { total = result.NumFound, results = previews });
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<IActionResult> GetByIsbn(string isbn, CancellationToken ct = default)
    {
        var edition = await _openLibraryService.GetByIsbnAsync(isbn, ct);
        if (edition is null)
            return NotFound(new { message = $"Book with ISBN {isbn} not found." });

        var preview = new OpenLibraryBookPreview
        {
            Key = edition.Key,
            Title = edition.Title,
            Authors = edition.Authors?.Select(a => a.Name).OfType<string>().ToArray() ?? Array.Empty<string>(),
            ISBNs = (edition.ISBN13 ?? new List<string>())
                .Concat(edition.ISBN10 ?? new List<string>())
                .ToArray(),
            FirstPublishYear = null,
            Categories = Array.Empty<string>(),
            CoverUrl = edition.CoverUrl is not null
                ? await _openLibraryService.GetCoverUrlAsync(edition.CoverUrl, ct)
                : null
        };

        if (!string.IsNullOrEmpty(preview.ISBNs?.FirstOrDefault()))
            preview.IsDuplicate = await _openLibraryService.IsDuplicateIsbnAsync(preview.ISBNs.First(), ct);

        return Ok(preview);
    }

    [HttpGet("cover/{coverEditionKey}")]
    public async Task<IActionResult> GetCover(string coverEditionKey, CancellationToken ct = default)
    {
        var coverUrl = await _openLibraryService.GetCoverUrlAsync(coverEditionKey, ct);
        if (coverUrl is null)
            return NotFound(new { message = "Cover not found." });
        return Ok(new { coverUrl });
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] OpenLibraryImportRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var isDuplicate = await _bookService.IsDuplicateIsbnAsync(request.ISBN, ct);
        if (isDuplicate)
            return Conflict(new { message = $"A book with ISBN {request.ISBN} already exists." });

        try
        {
            var book = await _bookService.CreateBookAsync(new BookRequest
            {
                Title = request.Title,
                ISBN = request.ISBN,
                Summary = request.Summary,
                CategoryId = request.CategoryId,
                AuthorIds = request.AuthorIds
            }, ct);

            return CreatedAtAction("Search", new { title = request.Title }, new OpenLibraryImportResponse
            {
                BookId = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                Created = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private static OpenLibraryBookPreview MapToPreview(OpenLibraryWork work)
    {
        return new OpenLibraryBookPreview
        {
            Key = work.Key,
            Title = work.Title,
            Subtitle = work.Subtitle,
            Authors = work.AuthorName?.ToArray() ?? Array.Empty<string>(),
            FirstPublishYear = work.FirstPublishYear,
            ISBNs = work.ISBN?.ToArray() ?? Array.Empty<string>(),
            Categories = work.Category?.ToArray() ?? Array.Empty<string>()
        };
    }
}
