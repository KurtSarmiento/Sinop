using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReservations(
        int page = 1,
        int pageSize = 20,
        string? userId = null,
        int? bookId = null,
        CancellationToken ct = default)
    {
        var result = await _reservationService.GetReservationsAsync(page, pageSize, userId, bookId, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReservation(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationService.GetReservationAsync(id, ct);
        if (reservation is null) return NotFound(new { message = $"Reservation {id} not found." });
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var reservation = await _reservationService.CreateReservationAsync(request, ct);
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelReservation(int id, CancellationToken ct = default)
    {
        // In a real scenario the userId would come from the authenticated user's claims.
        // For now, require it as a query parameter to enforce ownership.
        var userId = Request.Query["userId"].FirstOrDefault();
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "userId query parameter is required." });

        var cancelled = await _reservationService.CancelReservationAsync(id, userId, ct);
        if (!cancelled) return NotFound(new { message = $"Reservation {id} not found or cannot be cancelled." });
        return Ok(new { message = $"Reservation {id} cancelled." });
    }

    [HttpPost("process-expired")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> ProcessExpired(CancellationToken ct = default)
    {
        await _reservationService.ProcessExpiredReservationsAsync(ct);
        return Ok(new { message = "Expired reservations processed." });
    }

    [HttpPost("{id:int}/allocate-returned-copy")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> AllocateReturnedCopy(int id, [FromQuery] int copyId, CancellationToken ct = default)
    {
        var allocated = await _reservationService.AllocateReturnedCopyAsync(id, copyId, ct);
        if (!allocated)
            return Conflict(new { message = $"Copy {copyId} is not available or there is no queued reservation for book {id}." });
        return Ok(new { message = $"Returned copy {copyId} allocated to next queued borrower for book {id}." });
    }
}
