using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BasicBlazor.Web.Authorization;

/// <summary>
/// Custom authorization policy provider that dynamically creates policies for permissions.
/// Enables using permission names with PERM: prefix (e.g., "PERM:Page3:See_Button") directly
/// in [Authorize] and AuthorizeView without pre-registering each permission as a policy in Program.cs.
/// The PERM: prefix is stripped before checking against database permission names.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    /// <summary>
    /// Prefix used to identify permission-based policies.
    /// Example usage: Policy="PERM:Page3:See_Button"
    /// </summary>
    public const string PREFIX = "PERM:";

    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if policy name starts with PERM: prefix
        if (policyName.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            // Strip the PERM: prefix to get the actual permission name
            var permissionName = policyName.Substring(PREFIX.Length);

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissionName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // For other policies (like "RoleAccess"), use fallback provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
