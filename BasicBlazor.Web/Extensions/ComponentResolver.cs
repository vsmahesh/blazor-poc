using BasicBlazor.Data.Extensions;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace BasicBlazor.Web.Extensions;

/// <summary>
/// Singleton service that manages component override registrations.
/// Extensions register their component overrides during startup via RegisterComponentOverrides.
/// </summary>
public class ComponentResolver : IComponentResolver
{
    private readonly ConcurrentDictionary<string, Type> _overrides = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void RegisterOverride(string key, Type componentType)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Component key cannot be null or empty.", nameof(key));

        if (componentType == null)
            throw new ArgumentNullException(nameof(componentType));

        // Validate that the type is a Blazor component
        if (!typeof(IComponent).IsAssignableFrom(componentType))
        {
            throw new ArgumentException(
                $"Type '{componentType.FullName}' must implement IComponent to be registered as a component override.",
                nameof(componentType));
        }

        if (!_overrides.TryAdd(key, componentType))
        {
            // Allow override of existing registration (last extension wins)
            _overrides[key] = componentType;
            Console.WriteLine($"[ComponentResolver] Replaced override for key '{key}' with {componentType.FullName}");
        }
        else
        {
            Console.WriteLine($"[ComponentResolver] Registered override '{key}' -> {componentType.FullName}");
        }
    }

    /// <inheritdoc />
    public Type? GetOverride(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        return _overrides.TryGetValue(key, out var componentType) ? componentType : null;
    }

    /// <inheritdoc />
    public bool HasOverride(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return _overrides.ContainsKey(key);
    }
}
