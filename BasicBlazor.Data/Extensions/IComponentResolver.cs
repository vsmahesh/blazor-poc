namespace BasicBlazor.Data.Extensions;

/// <summary>
/// Service for registering and resolving component overrides.
/// Extensions can register their own component implementations to override default components.
/// </summary>
public interface IComponentResolver
{
    /// <summary>
    /// Registers a component override for the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for the component slot (e.g., "LoginFooter", "AppBrand").</param>
    /// <param name="componentType">The Type of the Blazor component to render.</param>
    void RegisterOverride(string key, Type componentType);

    /// <summary>
    /// Gets the component type registered for the specified key.
    /// </summary>
    /// <param name="key">The component slot identifier.</param>
    /// <returns>The registered component Type, or null if no override exists.</returns>
    Type? GetOverride(string key);

    /// <summary>
    /// Checks whether an override is registered for the specified key.
    /// </summary>
    /// <param name="key">The component slot identifier.</param>
    /// <returns>True if an override exists; otherwise, false.</returns>
    bool HasOverride(string key);
}
