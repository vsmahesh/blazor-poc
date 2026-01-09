namespace BasicBlazor.Data.Models;

/// <summary>
/// Represents a fine-grained permission in the system.
/// Permissions use a colon-separated naming convention (e.g., "Page3:See_Button").
/// </summary>
public class Permission
{
    /// <summary>
    /// Gets or sets the unique identifier for this permission.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the permission name using colon notation.
    /// Examples: "Page3:See_Button", "User:Edit", "Report:Export"
    /// Note: When used in policies, prefix with "PERM:" (e.g., Policy="PERM:Page3:See_Button")
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description of what this permission allows.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property: Many Permissions -> Many Roles (via RolePermission)
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
