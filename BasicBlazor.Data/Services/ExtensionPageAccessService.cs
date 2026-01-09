using System.Text.Json;
using BasicBlazor.Data.Configuration;
using BasicBlazor.Data.Extensions;

namespace BasicBlazor.Data.Services;

/// <summary>
/// Extended PageAccessService that loads configuration from extension if present.
/// If extension is active, loads extension's page-access.json (which replaces base config).
/// If no extension, loads base page-access.json.
/// Singleton service registered as PageAccessService in DI.
/// </summary>
public class ExtensionPageAccessService : PageAccessService
{
    /// <summary>
    /// Initializes a new instance of the ExtensionPageAccessService class.
    /// </summary>
    /// <param name="extensionLoader">The extension loader to check for active extensions.</param>
    public ExtensionPageAccessService(ExtensionLoader extensionLoader)
        : base(LoadConfiguration(extensionLoader))
    {
    }

    /// <summary>
    /// Loads page access configuration based on whether an extension is active.
    /// If extension exists, loads extension's configuration (replaces base).
    /// If no extension, loads base configuration.
    /// </summary>
    /// <param name="extensionLoader">The extension loader.</param>
    /// <returns>The loaded PageAccessConfiguration.</returns>
    private static PageAccessConfiguration LoadConfiguration(ExtensionLoader extensionLoader)
    {
        // Check if extension exists
        var extensionConfigPath = extensionLoader.GetExtensionConfigPath();

        if (!string.IsNullOrEmpty(extensionConfigPath) && File.Exists(extensionConfigPath))
        {
            // Extension exists - load its configuration (replaces base)
            return LoadConfigurationFromPath(extensionConfigPath);
        }

        // No extension - load base configuration
        var baseConfigPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "page-access.json");
        return LoadConfigurationFromPath(baseConfigPath);
    }

    /// <summary>
    /// Loads page access configuration from a specific file path.
    /// </summary>
    /// <param name="filePath">The path to the page-access.json file.</param>
    /// <returns>The loaded PageAccessConfiguration.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
    private static PageAccessConfiguration LoadConfigurationFromPath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Page access configuration file not found at: {filePath}");
        }

        var json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var configuration = JsonSerializer.Deserialize<PageAccessConfiguration>(json, options);

        if (configuration == null)
        {
            throw new InvalidOperationException($"Failed to deserialize page access configuration from: {filePath}");
        }

        return configuration;
    }
}
