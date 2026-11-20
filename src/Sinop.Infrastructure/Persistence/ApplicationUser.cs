namespace Sinop.Infrastructure.Persistence;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
{
    public string? FullName { get; set; }
}
