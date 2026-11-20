using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Domain.Enums;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize]
public class CopiesController : Controller
{
    private readonly BookCopyService _copyService;

    public CopiesController(BookCopyService copyService)
    {
        _copyService = copyService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, int? bookId = null, CopyStatus? status = null)
    {
        var result = await _copyService.GetCopiesAsync(page, pageSize, bookId, status);
        ViewData["BookId"] = bookId;
        ViewData["Status"] = status;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var copy = await _copyService.GetCopyAsync(id);
        if (copy is null) return NotFound();
        return View(copy);
    }

    [Authorize(Roles = "Librarian,Administrator")]
    public IActionResult Register() => View();

    [HttpPost]
    [Authorize(Roles = "Librarian,Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(BookCopyRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        try
        {
            await _copyService.RegisterCopyAsync(request);
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
    public async Task<IActionResult> Status(int id, CopyStatus status)
    {
        try
        {
            var updated = await _copyService.UpdateCopyStatusAsync(id, status);
            if (!updated) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
