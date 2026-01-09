using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BasicBlazor.Data.Services;

namespace BasicBlazor.Web.Authorization;

/// <summary>
/// Authorization handler that checks if a user's role has access to the current page.
/// Uses PageAccessService to validate against page-access.json configuration.
/// </summary>
public class RoleAccessHandler : AuthorizationHandler<RoleAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PageAccessService _pageAccessService;

    public RoleAccessHandler(IHttpContextAccessor httpContextAccessor, PageAccessService pageAccessService)
    {
        _httpContextAccessor = httpContextAccessor;
        _pageAccessService = pageAccessService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleAccessRequirement requirement)
    {
        // Get the current HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Get the current page path
        var pagePath = httpContext.Request.Path.Value?.ToLower() ?? "";

        // Get the user's role
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userRole))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if the user's role has access to this page
        if (_pageAccessService.IsPageAccessible(pagePath, userRole))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}