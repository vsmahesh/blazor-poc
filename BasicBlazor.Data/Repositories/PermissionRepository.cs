using BasicBlazor.Data.Data;
using BasicBlazor.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BasicBlazor.Data.Repositories;

/// <summary>
/// Repository for permission-related data access operations.
/// </summary>
public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<string>> GetPermissionsByRoleIdAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.PermissionName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(int roleId, string permissionName)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.PermissionName == permissionName);
    }

    /// <inheritdoc />
    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .OrderBy(p => p.PermissionName)
            .ToListAsync();
    }
}
