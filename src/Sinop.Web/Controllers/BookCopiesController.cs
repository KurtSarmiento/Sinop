using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Domain.Enums;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class BookCopiesController : ControllerBase
{
    private readonly BookCopyService _copyService;

    public BookCopiesController(BookCopyService copyService)
    {
        _copyService = copyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCopies(
        int page = 1,
        int pageSize = 20,
        int? bookId = null,
        CopyStatus? status = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        var result = await _copyService.GetCopiesAsync(page, pageSize, bookId, status, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCopy(int id, CancellationToken ct = default)
    {
        var copy = await _copyService.GetCopyAsync(id, ct);
        if (copy is null) return NotFound(new { message = $"Copy {id} not found." });
        return Ok(copy);
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> RegisterCopy([FromBody] BookCopyRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var copy = await _copyService.RegisterCopyAsync(request, ct);
            return CreatedAtAction(nameof(GetCopy), new { id = copy.Id }, copy);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> UpdateCopyStatus(int id, [FromBody] CopyStatusUpdate request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var updated = await _copyService.UpdateCopyStatusAsync(id, request.Status, ct);
            if (!updated) return NotFound(new { message = $"Copy {id} not found." });
            return Ok(new { message = $"Copy {id} status updated to {request.Status}." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("accession-number/{bookId:int}")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> GenerateAccessionNumber(int bookId, CancellationToken ct = default)
    {
        var accession = await _copyService.GenerateAccessionNumberAsync(bookId, ct);
        return Ok(new { accessionNumber = accession });
    }
}

public class CopyStatusUpdate
{
    public CopyStatus Status { get; set; }
}
