using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize]
public class ReservationsController : Controller
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? userId = null, int? bookId = null)
    {
        var result = await _reservationService.GetReservationsAsync(page, pageSize, userId, bookId);
        ViewData["UserId"] = userId;
        ViewData["BookId"] = bookId;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetReservationAsync(id);
        if (reservation is null) return NotFound();
        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string userId)
    {
        var cancelled = await _reservationService.CancelReservationAsync(id, userId);
        if (!cancelled) return NotFound();
        return RedirectToAction(nameof(Index), new { userId });
    }
}
