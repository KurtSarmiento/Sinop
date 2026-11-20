using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize]
public class CirculationController : Controller
{
    private readonly LoanService _loanService;
    private readonly BookCopyService _copyService;

    public CirculationController(LoanService loanService, BookCopyService copyService)
    {
        _loanService = loanService;
        _copyService = copyService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? userId = null)
    {
        var result = await _loanService.GetActiveLoansAsync(page, pageSize, userId);
        ViewData["UserId"] = userId;
        return View(result);
    }

    public IActionResult Checkout() => View();

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(LoanRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        try
        {
            await _loanService.CheckoutAsync(request);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int loanId)
    {
        try
        {
            await _loanService.ReturnAsync(loanId);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
