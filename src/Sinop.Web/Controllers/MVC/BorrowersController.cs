using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize]
public class BorrowersController : Controller
{
    private readonly BorrowerService _borrowerService;

    public BorrowersController(BorrowerService borrowerService)
    {
        _borrowerService = borrowerService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var result = await _borrowerService.GetBorrowersAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(string userId)
    {
        var borrower = await _borrowerService.GetBorrowerAsync(userId);
        if (borrower is null) return NotFound();
        return View(borrower);
    }

    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> History(string userId)
    {
        var result = await _borrowerService.GetBorrowerHistoryAsync(userId);
        ViewData["UserId"] = userId;
        return View(result);
    }
}
