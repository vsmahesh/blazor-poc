namespace BasicBlazor.Data.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    // Navigation property: Many Users -> One Role
    public Role Role { get; set; } = null!;
}
