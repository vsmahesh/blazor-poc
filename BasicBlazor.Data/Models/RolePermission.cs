namespace BasicBlazor.Data.Models;

/// <summary>
/// Join entity for the many-to-many relationship between Roles and Permissions.
/// Allows flexible assignment of permissions to roles without hardcoding.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Gets or sets the unique identifier for this role-permission relationship.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the Role.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the Permission.
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Navigation property to the Role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Navigation property to the Permission.
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
