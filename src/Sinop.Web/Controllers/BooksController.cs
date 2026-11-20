using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;

    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        var result = await _bookService.GetBooksAsync(page, pageSize, search, null, null, false, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBook(int id, CancellationToken ct = default)
    {
        var book = await _bookService.GetBookDetailsAsync(id, ct);
        if (book is null) return NotFound(new { message = $"Book {id} not found." });
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> CreateBook([FromBody] BookRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var book = await _bookService.CreateBookAsync(request, ct);
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] BookRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var book = await _bookService.UpdateBookAsync(id, request, ct);
            if (book is null) return NotFound(new { message = $"Book {id} not found." });
            return Ok(book);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> ArchiveBook(int id, CancellationToken ct = default)
    {
        try
        {
            var archived = await _bookService.ArchiveBookAsync(id, ct);
            if (!archived) return NotFound(new { message = $"Book {id} not found." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
