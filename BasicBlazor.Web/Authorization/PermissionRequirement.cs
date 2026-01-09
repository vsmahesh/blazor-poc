using Microsoft.AspNetCore.Authorization;

namespace BasicBlazor.Web.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control.
/// Used to check if a user has a specific permission.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the permission name required for authorization.
    /// </summary>
    public string PermissionName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permissionName">The permission name (e.g., "Page3:See_Button").</param>
    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName ?? throw new ArgumentNullException(nameof(permissionName));
    }
}
