using BasicBlazor.Data.Models;

namespace BasicBlazor.Data.Repositories;

/// <summary>
/// Repository interface for permission-related data access operations.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Gets all permission names for a specific role.
    /// </summary>
    /// <param name="roleId">The role ID to query permissions for.</param>
    /// <returns>List of permission names (e.g., "Page3:See_Button").</returns>
    Task<List<string>> GetPermissionsByRoleIdAsync(int roleId);

    /// <summary>
    /// Checks if a role has a specific permission.
    /// </summary>
    /// <param name="roleId">The role ID to check.</param>
    /// <param name="permissionName">The permission name to check for.</param>
    /// <returns>True if the role has the permission, false otherwise.</returns>
    Task<bool> HasPermissionAsync(int roleId, string permissionName);

    /// <summary>
    /// Gets all permissions in the system.
    /// </summary>
    /// <returns>List of all permissions.</returns>
    Task<List<Permission>> GetAllPermissionsAsync();
}
