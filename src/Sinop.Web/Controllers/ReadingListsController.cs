using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class ReadingListsController : ControllerBase
{
    private readonly ReadingListService _readingListService;

    public ReadingListsController(ReadingListService readingListService)
    {
        _readingListService = readingListService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReadingLists(
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        var result = await _readingListService.GetReadingListsAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReadingList(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        var list = await _readingListService.GetReadingListAsync(id, userId, ct);
        if (list is null) return NotFound(new { message = $"Reading list {id} not found or access denied." });
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReadingList([FromBody] ReadingListRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        var list = await _readingListService.CreateReadingListAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetReadingList), new { id = list.Id }, list);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReadingList(int id, [FromBody] ReadingListRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        var updated = await _readingListService.UpdateReadingListAsync(id, userId, request, ct);
        if (!updated) return NotFound(new { message = $"Reading list {id} not found or access denied." });
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReadingList(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        var deleted = await _readingListService.DeleteReadingListAsync(id, userId, ct);
        if (!deleted) return NotFound(new { message = $"Reading list {id} not found or access denied." });
        return NoContent();
    }

    [HttpPost("{listId:int}/items")]
    public async Task<IActionResult> AddItem(int listId, [FromBody] AddReadingListItemRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        try
        {
            var list = await _readingListService.AddItemAsync(listId, userId, request, ct);
            return Ok(list);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{listId:int}/items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int listId, int itemId, CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Unable to determine user identity." });

        var removed = await _readingListService.RemoveItemAsync(listId, itemId, userId, ct);
        if (!removed) return NotFound(new { message = $"Item {itemId} not found or access denied." });
        return NoContent();
    }
}
