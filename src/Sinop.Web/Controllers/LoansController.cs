using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinop.Web.DTOs;
using Sinop.Web.Services;

namespace Sinop.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class LoansController : ControllerBase
{
    private readonly LoanService _loanService;

    public LoansController(LoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveLoans(
        int page = 1,
        int pageSize = 20,
        string? userId = null,
        CancellationToken ct = default)
    {
        var result = await _loanService.GetActiveLoansAsync(page, pageSize, userId, ct);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetLoanHistory(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _loanService.GetLoanHistoryAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("eligibility/{userId}")]
    public async Task<IActionResult> GetEligibility(string userId, CancellationToken ct = default)
    {
        var eligible = await _loanService.ValidateBorrowerEligibilityAsync(userId, ct);
        return Ok(new { userId, eligible, maxActiveLoans = LibraryRules.MaxSimultaneousActiveLoans });
    }

    [HttpGet("overdue-count")]
    public async Task<IActionResult> GetOverdueCount(CancellationToken ct = default)
    {
        var count = await _loanService.GetOverdueCountAsync(ct);
        return Ok(new { overdueCount = count });
    }

    [HttpPost("checkout")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> Checkout([FromBody] LoanRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var loan = await _loanService.CheckoutAsync(request, ct);
            return CreatedAtAction(nameof(GetLoanHistory), new { userId = request.UserId }, loan);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("return")]
    [Authorize(Roles = "Librarian,Administrator")]
    public async Task<IActionResult> Return([FromBody] ReturnRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var loan = await _loanService.ReturnAsync(request.LoanId, ct);
            return Ok(loan);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
