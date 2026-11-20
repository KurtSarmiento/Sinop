using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Sinop.Web.Jobs;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        // Restrict Hangfire Dashboard to authenticated administrators only.
        return httpContext.User?.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Administrator");
    }
}
