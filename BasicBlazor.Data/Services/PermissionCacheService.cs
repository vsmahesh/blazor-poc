using BasicBlazor.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BasicBlazor.Data.Services;

/// <summary>
/// Service for caching role permissions in memory to improve authorization performance.
/// Caches the mapping of role IDs to their permission sets with a configurable TTL.
/// </summary>
public class PermissionCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public PermissionCacheService(IMemoryCache cache, IServiceProvider serviceProvider)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Checks if a role has a specific permission with caching.
    /// </summary>
    /// <param name="roleId">The role ID to check.</param>
    /// <param name="permissionName">The permission name to check for.</param>
    /// <returns>True if the role has the permission, false otherwise.</returns>
    public async Task<bool> HasPermissionAsync(int roleId, string permissionName)
    {
        var cacheKey = $"role_{roleId}_permissions";

        // Try to get from cache
        if (!_cache.TryGetValue(cacheKey, out HashSet<string>? permissions))
        {
            // Cache miss - load from database
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
            var permissionList = await repository.GetPermissionsByRoleIdAsync(roleId);

            permissions = new HashSet<string>(permissionList, StringComparer.OrdinalIgnoreCase);

            // Store in cache with 10-minute expiration
            _cache.Set(cacheKey, permissions, _cacheExpiration);
        }

        return permissions?.Contains(permissionName) ?? false;
    }

    /// <summary>
    /// Invalidates cached permissions for a specific role.
    /// Call this when role permissions are updated.
    /// </summary>
    /// <param name="roleId">The role ID whose cache should be invalidated.</param>
    public void InvalidateRoleCache(int roleId)
    {
        var cacheKey = $"role_{roleId}_permissions";
        _cache.Remove(cacheKey);
    }

    /// <summary>
    /// Clears all permission caches.
    /// Use this when permissions are updated globally.
    /// </summary>
    public void ClearAllCaches()
    {
        // Note: IMemoryCache doesn't have a built-in clear method
        // In production, consider using a custom cache wrapper that tracks keys
        // For now, this is a placeholder for future implementation
    }
}
