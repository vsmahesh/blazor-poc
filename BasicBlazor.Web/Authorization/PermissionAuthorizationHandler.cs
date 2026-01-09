using BasicBlazor.Data.Data;
using BasicBlazor.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BasicBlazor.Web.Authorization;

/// <summary>
/// Authorization handler for permission-based access control.
/// Checks if a user's role has the required permission using cached lookups.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly PermissionCacheService _permissionCache;
    private readonly AppDbContext _context;

    public PermissionAuthorizationHandler(
        PermissionCacheService permissionCache,
        AppDbContext context)
    {
        _permissionCache = permissionCache ?? throw new ArgumentNullException(nameof(permissionCache));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user's role from claims
        var roleName = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleName))
        {
            return;
        }

        // Find role ID from database
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName);

        if (role == null)
        {
            return;
        }

        // Check permission using cache service
        var hasPermission = await _permissionCache.HasPermissionAsync(role.Id, requirement.PermissionName);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
