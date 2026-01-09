namespace BasicBlazor.Data.Models;

public class Role
{
    public int Id { get; set; }

    public string RoleName { get; set; } = string.Empty;

    // Navigation property: One Role -> Many Users
    public ICollection<User> Users { get; set; } = new List<User>();

    // Navigation property: Many Roles -> Many Permissions (via RolePermission)
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
