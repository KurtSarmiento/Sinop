using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize(Roles = "Librarian,Administrator")]
public class ReportsController : Controller
{
    private readonly LoanService _loanService;
    private readonly BookCopyService _copyService;
    private readonly ReservationService _reservationService;

    public ReportsController(LoanService loanService, BookCopyService copyService, ReservationService reservationService)
    {
        _loanService = loanService;
        _copyService = copyService;
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Index()
    {
        var activeLoans = await _loanService.GetActiveLoansAsync(1, 1000);
        var overdueCount = await _loanService.GetOverdueCountAsync();
        var copies = await _copyService.GetCopiesAsync(1, 1000);

        ViewData["ActiveLoans"] = activeLoans.TotalCount;
        ViewData["OverdueCount"] = overdueCount;
        ViewData["TotalCopies"] = copies.TotalCount;
        ViewData["AvailableCopies"] = copies.Items.Count(c => c.Status == Sinop.Domain.Enums.CopyStatus.Available);
        ViewData["BorrowedCopies"] = copies.Items.Count(c => c.Status == Sinop.Domain.Enums.CopyStatus.Borrowed);
        ViewData["PendingReservations"] = (await _reservationService.GetReservationsAsync(1, 1000)).Items.Count(r => r.Status == Sinop.Domain.Enums.ReservationStatus.Queued);

        return View();
    }
}
