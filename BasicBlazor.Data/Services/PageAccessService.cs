using System.Text.Json;
using BasicBlazor.Data.Configuration;

namespace BasicBlazor.Data.Services;

/// <summary>
/// Service for managing role-based page access authorization.
/// Loads configuration from page-access.json and caches it for performance.
/// Registered as Singleton for caching efficiency.
/// </summary>
public class PageAccessService
{
    private readonly PageAccessConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the PageAccessService class.
    /// Loads and parses the page-access.json configuration file.
    /// </summary>
    public PageAccessService() : this(LoadConfigurationFromFile())
    {
    }

    /// <summary>
    /// Initializes a new instance of the PageAccessService class with a provided configuration.
    /// Used for testing to avoid file I/O.
    /// </summary>
    /// <param name="configuration">The page access configuration to use.</param>
    internal PageAccessService(PageAccessConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Determines if a specific role can access a given page.
    /// </summary>
    /// <param name="pagePath">The page path to check (e.g., "/page1").</param>
    /// <param name="roleName">The role name to check (e.g., "User").</param>
    /// <returns>True if the role can access the page; otherwise, false.</returns>
    public bool IsPageAccessible(string pagePath, string? roleName)
    {
        // Return false if role name is null or empty
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        // Find the page in configuration by path
        var page = _configuration.PageAccess.FirstOrDefault(p => p.PagePath == pagePath);

        // Return false if page not found
        if (page == null)
        {
            return false;
        }

        // Check if the role is in the allowed roles list (case-sensitive)
        return page.AllowedRoles.Contains(roleName);
    }

    /// <summary>
    /// Gets all pages accessible to a specific role, ordered by the order property.
    /// Used for rendering dynamic navigation menus.
    /// </summary>
    /// <param name="roleName">The role name to get accessible pages for.</param>
    /// <returns>List of PageAccessRule objects for accessible pages, ordered by Order property.</returns>
    public List<PageAccessRule> GetAllowedPagesForRole(string? roleName)
    {
        // Return empty list if role name is null or empty
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return new List<PageAccessRule>();
        }

        // Filter pages where AllowedRoles contains the roleName, and order by Order property
        return _configuration.PageAccess
            .Where(p => p.AllowedRoles.Contains(roleName))
            .OrderBy(p => p.Order)
            .ToList();
    }

    /// <summary>
    /// Gets the first (lowest order) page accessible to a specific role.
    /// Used for post-login redirect logic.
    /// </summary>
    /// <param name="roleName">The role name to get the first allowed page for.</param>
    /// <returns>The page path (e.g., "/page1"), or null if no pages are accessible.</returns>
    public string? GetFirstAllowedPage(string? roleName)
    {
        // Get all allowed pages for the role (already filtered and sorted)
        var allowedPages = GetAllowedPagesForRole(roleName);

        // Return the PagePath of the first item, or null if no pages
        return allowedPages.FirstOrDefault()?.PagePath;
    }

    /// <summary>
    /// Loads the page access configuration from the page-access.json file.
    /// </summary>
    /// <returns>The loaded PageAccessConfiguration.</returns>
    /// <exception cref="FileNotFoundException">Thrown if page-access.json is not found.</exception>
    /// <exception cref="JsonException">Thrown if the JSON file is malformed.</exception>
    private static PageAccessConfiguration LoadConfigurationFromFile()
    {
        // Construct file path relative to application base directory
        var filePath = Path.Combine(AppContext.BaseDirectory, "Configuration", "page-access.json");

        // Check if file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Page access configuration file not found at: {filePath}");
        }

        // Read JSON content from file
        var json = File.ReadAllText(filePath);

        // Deserialize JSON to configuration object
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var configuration = JsonSerializer.Deserialize<PageAccessConfiguration>(json, options);

        // Ensure configuration is not null
        if (configuration == null)
        {
            throw new InvalidOperationException("Failed to deserialize page access configuration.");
        }

        return configuration;
    }
}
