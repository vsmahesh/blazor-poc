using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BasicBlazor.Data.Data;
using BasicBlazor.Data.Repositories;
using BasicBlazor.Data.Services;

namespace BasicBlazor.Data.Extensions;

/// <summary>
/// Extension methods for configuring Data layer services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Data layer services including DbContext, repositories, and business services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The IServiceCollection for method chaining.</returns>
    /// <remarks>
    /// This method registers:
    /// - Database context (Scoped): AppDbContext with SQLite provider
    /// - Infrastructure: IMemoryCache for permission caching
    /// - Repositories (Scoped): IUserRepository, IPermissionRepository
    /// - Business Services:
    ///   - AuthService (Scoped) - User authentication
    ///   - PageAccessService (Singleton) - Page-role mapping from JSON config
    ///   - PermissionCacheService (Singleton) - Cached permission lookups
    ///
    /// Requires configuration key: "ConnectionStrings:DefaultConnection"
    /// </remarks>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Database Context
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // 2. Infrastructure - Memory cache for permission caching
        services.AddMemoryCache();

        // 3. Repositories (Scoped - need fresh DbContext per request)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // 4. Business Services
        // AuthService is Scoped because it depends on IUserRepository (which is Scoped)
        services.AddScoped<AuthService>();

        // PageAccessService is Singleton because it loads page-access.json once and never changes
        services.AddSingleton<PageAccessService>();

        // PermissionCacheService is Singleton and uses IServiceProvider to create scopes for DB access
        services.AddSingleton<PermissionCacheService>();

        return services;
    }

    /// <summary>
    /// Registers all Data layer services with extension support.
    /// Discovers and initializes client extensions if present.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The IServiceCollection for method chaining.</returns>
    /// <remarks>
    /// This method:
    /// - Registers all base data services via AddDataServices
    /// - Discovers and loads client extensions (BasicBlazor.Extension.*)
    /// - Replaces PageAccessService with ExtensionPageAccessService for config loading
    /// - Calls extension's ConfigureServices if present
    ///
    /// Extension's page-access.json REPLACES the base configuration when present.
    /// </remarks>
    public static IServiceCollection AddDataServicesWithExtensions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Register base data services first
        services.AddDataServices(configuration);

        // 2. Create and initialize extension loader
        var extensionLoader = new ExtensionLoader();
        extensionLoader.DiscoverExtension();

        // 3. Register ExtensionLoader as Singleton
        services.AddSingleton(extensionLoader);

        // 4. Replace PageAccessService with ExtensionPageAccessService
        // Remove the original PageAccessService registration
        var pageAccessDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(PageAccessService));
        if (pageAccessDescriptor != null)
        {
            services.Remove(pageAccessDescriptor);
        }

        // Register ExtensionPageAccessService as PageAccessService (Singleton)
        // This will load extension's config if present, otherwise base config
        services.AddSingleton<PageAccessService>(sp =>
        {
            var loader = sp.GetRequiredService<ExtensionLoader>();
            return new ExtensionPageAccessService(loader);
        });

        // 5. Call extension's ConfigureServices if present
        if (extensionLoader.ActiveExtension != null)
        {
            extensionLoader.ActiveExtension.ConfigureServices(services, configuration);
        }

        return services;
    }

    /// <summary>
    /// Applies Entity Framework Core migrations to the database.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <remarks>
    /// This method creates a scope, resolves the AppDbContext, and applies any pending migrations.
    /// Suitable for development environments. In production, consider using explicit migration commands.
    /// </remarks>
    public static void ApplyDataMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        // Seed data is handled in AppDbContext.OnModelCreating()
    }
}
