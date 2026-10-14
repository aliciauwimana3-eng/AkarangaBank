using Hangfire.Dashboard;

namespace AkarangaBank.Web.Services;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => context.GetHttpContext().User.IsInRole("admin");
}
