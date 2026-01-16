using System.Reflection;
using BasicBlazor.Data.Extensions;
using BasicBlazor.Extension.ClientA.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicBlazor.Extension.ClientA;

/// <summary>
/// ClientA extension implementation.
/// Provides featured products functionality specific to ClientA.
/// </summary>
public class ClientAExtension : IClientExtension
{
    /// <summary>
    /// Gets the unique client identifier.
    /// </summary>
    public string ClientId => "ClientA";

    /// <summary>
    /// Gets the display name for this client extension.
    /// </summary>
    public string DisplayName => "Client A - Featured Products";

    /// <summary>
    /// Gets the assembly containing extension components.
    /// </summary>
    public Assembly ComponentAssembly => typeof(ClientAExtension).Assembly;

    /// <summary>
    /// Registers client-specific services in the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register FeaturedProductsService for managing product list
        services.AddScoped<FeaturedProductsService>();

        // Additional client-specific services can be registered here
        // For example:
        // - services.AddScoped<ClientASpecificService>();
        // - services.AddSingleton<ClientAConfigService>();
        // - Could also inject AppDbContext for database operations
    }

    /// <summary>
    /// Registers component overrides for Client A.
    /// </summary>
    /// <param name="resolver">The component resolver.</param>
    public void RegisterComponentOverrides(IComponentResolver resolver)
    {
        // Override the app brand on login page
        resolver.RegisterOverride("AppBrand", typeof(Components.ClientAAppBrand));

        // Add a login footer (no default exists, so this adds new content)
        resolver.RegisterOverride("LoginFooter", typeof(Components.LoginFooter));
    }
}
