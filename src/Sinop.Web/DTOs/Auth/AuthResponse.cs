namespace Sinop.Web.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DateTime ExpiresAt { get; set; }
}
