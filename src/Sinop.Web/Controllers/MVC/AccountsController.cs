using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize(Roles = "Librarian,Administrator")]
public class AccountsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountsController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.GetUsersInRoleAsync("Administrator");
        var librarians = await _userManager.GetUsersInRoleAsync("Librarian");
        var borrowers = await _userManager.GetUsersInRoleAsync("Borrower");

        var accounts = new Dictionary<string, IList<ApplicationUser>>
        {
            ["Administrators"] = users,
            ["Staff"] = librarians,
            ["Borrowers"] = borrowers
        };

        ViewData["IsAdministrator"] = User.IsInRole("Administrator");
        return View(accounts);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["IsAdministrator"] = User.IsInRole("Administrator");
        return View(new AccountCreateRequest
        {
            Role = User.IsInRole("Administrator") ? "Borrower" : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountCreateRequest request)
    {
        ViewData["IsAdministrator"] = User.IsInRole("Administrator");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Complete all required fields correctly before creating the account.";
            return View(request);
        }

        var role = User.IsInRole("Administrator")
            ? request.Role
            : "Borrower";

        if (role is not ("Administrator" or "Librarian" or "Borrower"))
        {
            ModelState.AddModelError(nameof(request.Role), "Select a valid account role.");
            return View(request);
        }

        if (await _userManager.FindByEmailAsync(request.Email.Trim()) is not null)
        {
            TempData["Error"] = "An account already uses this email address. Use a different email.";
            ModelState.AddModelError(nameof(request.Email), "This email is already registered.");
            return View(request);
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            TempData["Error"] = "The account could not be created. Check the password requirements and required account fields.";
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(request);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(request);
        }

        TempData["Success"] = $"Account created successfully for {request.FullName.Trim()} as {role}.";
        return RedirectToAction(nameof(Index));
    }
}
