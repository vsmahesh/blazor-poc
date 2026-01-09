using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicBlazor.Data.Extensions;

/// <summary>
/// Interface that all client extensions must implement.
/// Provides hooks for service registration, component discovery, and metadata.
/// </summary>
public interface IClientExtension
{
    /// <summary>
    /// Gets the unique client identifier (e.g., "ClientA", "ClientB").
    /// Used for logging and diagnostics.
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Gets the display name for this client extension.
    /// Used for logging and diagnostics.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the assembly containing the extension's Blazor components.
    /// Used by the Router for discovering pages.
    /// </summary>
    Assembly ComponentAssembly { get; }

    /// <summary>
    /// Registers extension-specific services in the DI container.
    /// Called during application startup after base services are registered.
    /// </summary>
    /// <param name="services">The service collection to register services in.</param>
    /// <param name="configuration">The application configuration.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
