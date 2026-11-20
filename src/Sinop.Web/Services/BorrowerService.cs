using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sinop.Domain.Entities;
using Sinop.Domain.Enums;
using Sinop.Infrastructure.Persistence;
using Sinop.Web.DTOs;

namespace Sinop.Web.Services;

public class BorrowerService
{
    private readonly LibraryDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BorrowerService(LibraryDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<PagedResult<BorrowerResponse>> GetBorrowersAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var users = _context.Users.AsNoTracking();
        var total = await users.CountAsync(ct);
        var items = await users
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new List<BorrowerResponse>();
        foreach (var user in items)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(await MapToResponseAsync(user, roles, ct));
        }

        return new PagedResult<BorrowerResponse> { Items = result, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<BorrowerResponse?> GetBorrowerAsync(string userId, CancellationToken ct = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return await MapToResponseAsync(user, roles, ct);
    }

    public async Task<bool> ValidateAccountAsync(string userId, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, ct);
        return user is not null && !user.LockoutEnabled;
    }

    public async Task<PagedResult<BorrowerResponse>> GetBorrowerHistoryAsync(string userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        // Borrower history = their loans; reuse borrower shape with loan-based counts.
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return new PagedResult<BorrowerResponse> { Page = page, PageSize = pageSize };

        var roles = await _userManager.GetRolesAsync(user);
        var item = await MapToResponseAsync(user, roles, ct);

        return new PagedResult<BorrowerResponse> { Items = new List<BorrowerResponse> { item }, Page = page, PageSize = pageSize, TotalCount = 1 };
    }

    private async Task<BorrowerResponse> MapToResponseAsync(ApplicationUser user, IList<string> roles, CancellationToken ct)
    {
        var activeLoans = await _context.Loans.CountAsync(l => l.UserId == user.Id && l.Status == LoanStatus.Active, ct);
        var activeReservations = await _context.Reservations.CountAsync(r => r.UserId == user.Id && r.Status == ReservationStatus.Queued, ct);
        return new BorrowerResponse
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = string.Join(", ", roles),
            ActiveLoans = activeLoans,
            ActiveReservations = activeReservations
        };
    }
}
