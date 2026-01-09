using System.Reflection;

namespace BasicBlazor.Data.Extensions;

/// <summary>
/// Service for discovering and loading client extensions at runtime.
/// Singleton service that scans loaded assemblies for IClientExtension implementations.
/// </summary>
public class ExtensionLoader
{
    private IClientExtension? _activeExtension;

    /// <summary>
    /// Gets the currently active client extension, if any.
    /// </summary>
    public IClientExtension? ActiveExtension => _activeExtension;

    /// <summary>
    /// Discovers and loads the active client extension from loaded assemblies.
    /// Looks for types implementing IClientExtension in assemblies with
    /// names matching "BasicBlazor.Extension.*".
    /// </summary>
    /// <returns>True if an extension was found and loaded; otherwise, false.</returns>
    public bool DiscoverExtension()
    {
        Console.WriteLine("[ExtensionLoader] Starting extension discovery...");

        // First, explicitly load any extension assemblies from the bin folder
        // This ensures they're in the AppDomain before we scan for them
        LoadExtensionAssembliesFromDisk();

        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Console.WriteLine($"[ExtensionLoader] Total assemblies loaded: {assemblies.Length}");

        // Find extension assemblies (naming convention: BasicBlazor.Extension.*)
        var extensionAssemblies = assemblies
            .Where(a => a.GetName().Name?.StartsWith("BasicBlazor.Extension.") == true)
            .ToList();

        Console.WriteLine($"[ExtensionLoader] Found {extensionAssemblies.Count} extension assemblies");

        // Should only have 0 or 1 extension assembly (enforced by build configuration)
        if (extensionAssemblies.Count == 0)
        {
            Console.WriteLine("[ExtensionLoader] No extension assemblies found");
            return false;
        }

        if (extensionAssemblies.Count > 1)
        {
            var names = string.Join(", ", extensionAssemblies.Select(a => a.GetName().Name));
            throw new InvalidOperationException(
                $"Multiple client extensions detected: {names}. Only one extension can be active at a time.");
        }

        var extensionAssembly = extensionAssemblies.Single();

        // Find types implementing IClientExtension
        var extensionType = extensionAssembly.GetTypes()
            .FirstOrDefault(t => typeof(IClientExtension).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        if (extensionType == null)
        {
            throw new InvalidOperationException(
                $"Extension assembly '{extensionAssembly.GetName().Name}' does not contain a class implementing IClientExtension.");
        }

        // Create instance
        _activeExtension = (IClientExtension?)Activator.CreateInstance(extensionType);

        if (_activeExtension == null)
        {
            throw new InvalidOperationException(
                $"Failed to create instance of extension type '{extensionType.FullName}'.");
        }

        Console.WriteLine($"[ExtensionLoader] Successfully loaded extension: {_activeExtension.ClientId}");
        Console.WriteLine($"[ExtensionLoader] Extension assembly: {_activeExtension.ComponentAssembly.FullName}");

        return true;
    }

    /// <summary>
    /// Gets the page access configuration file path for the active extension.
    /// </summary>
    /// <returns>The full path to the extension's page-access.json, or null if no extension is active.</returns>
    public string? GetExtensionConfigPath()
    {
        if (_activeExtension == null)
        {
            return null;
        }

        var assemblyLocation = _activeExtension.ComponentAssembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(assemblyDir))
        {
            return null;
        }

        return Path.Combine(assemblyDir, _activeExtension.PageAccessConfigPath);
    }

    /// <summary>
    /// Static helper method to get all extension assemblies for early registration (e.g., in Program.cs).
    /// This method loads extension assemblies from disk and returns them.
    /// Should be called during application startup before service configuration.
    /// </summary>
    /// <returns>Array of extension assemblies found, or empty array if none found.</returns>
    public static Assembly[] GetExtensionAssemblies()
    {
        Console.WriteLine("[ExtensionLoader.Static] Getting extension assemblies for early registration...");

        // First, explicitly load any extension assemblies from disk
        LoadExtensionAssembliesFromDisk();

        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Find extension assemblies (naming convention: BasicBlazor.Extension.*)
        var extensionAssemblies = assemblies
            .Where(a => a.GetName().Name?.StartsWith("BasicBlazor.Extension.") == true)
            .ToArray();

        Console.WriteLine($"[ExtensionLoader.Static] Found {extensionAssemblies.Length} extension assemblies for registration");
        foreach (var assembly in extensionAssemblies)
        {
            Console.WriteLine($"[ExtensionLoader.Static]   - {assembly.GetName().Name}");
        }

        return extensionAssemblies;
    }

    /// <summary>
    /// Explicitly loads extension assemblies from the application's base directory.
    /// This ensures extension assemblies are loaded into the AppDomain before discovery.
    /// </summary>
    private static void LoadExtensionAssembliesFromDisk()
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"[ExtensionLoader] Scanning for extensions in: {baseDirectory}");

            // Look for DLL files matching the extension naming pattern
            var extensionFiles = Directory.GetFiles(baseDirectory, "BasicBlazor.Extension.*.dll", SearchOption.TopDirectoryOnly);
            Console.WriteLine($"[ExtensionLoader] Found {extensionFiles.Length} extension DLL files");

            foreach (var extensionFile in extensionFiles)
            {
                try
                {
                    Console.WriteLine($"[ExtensionLoader] Loading assembly: {Path.GetFileName(extensionFile)}");
                    // Load the assembly into the AppDomain
                    // Use LoadFrom to load from the specific path
                    var loadedAssembly = Assembly.LoadFrom(extensionFile);
                    Console.WriteLine($"[ExtensionLoader] Successfully loaded: {loadedAssembly.FullName}");
                }
                catch (Exception ex)
                {
                    // Log or ignore individual assembly load failures
                    // This prevents one bad extension from breaking everything
                    Console.WriteLine($"[ExtensionLoader] Warning: Failed to load extension assembly {Path.GetFileName(extensionFile)}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // If we can't scan the directory, just continue with already-loaded assemblies
            Console.WriteLine($"[ExtensionLoader] Warning: Failed to scan for extension assemblies: {ex.Message}");
        }
    }
}
