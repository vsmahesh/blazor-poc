using Microsoft.AspNetCore.Authorization;

namespace BasicBlazor.Web.Authorization;

/// <summary>
/// Authorization requirement for role-based page access.
/// Checks if the user's role has access to the current page based on page-access.json configuration.
/// </summary>
public class RoleAccessRequirement : IAuthorizationRequirement
{
    // No additional properties needed - the handler will check dynamically based on the request path
}