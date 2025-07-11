using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HomeFinancial.WebApi.Auth;

/// <summary>
/// Обработчик требования OwnDataRequirement.
/// </summary>
public class OwnDataHandler : AuthorizationHandler<OwnDataRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OwnDataHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnDataRequirement requirement)
    {
        var userIdFromToken = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userIdFromRoute = _httpContextAccessor.HttpContext?.Request.RouteValues["userId"]?.ToString();

        if (!string.IsNullOrEmpty(userIdFromToken) && userIdFromToken == userIdFromRoute)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
