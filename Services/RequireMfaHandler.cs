using Microsoft.AspNetCore.Authorization;

namespace AkarangaBank.Web.Services;

public class RequireMfa : IAuthorizationRequirement
{
}

// Autorise seulement les utilisateurs connectés avec un second facteur (claim amr = mfa)
public class RequireMfaHandler : AuthorizationHandler<RequireMfa>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireMfa requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (context.User.FindAll("amr").Any(c => c.Value == "mfa"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
