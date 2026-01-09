using System.Reflection;
using BasicBlazor.Data.Extensions;

namespace BasicBlazor.Web.Extensions;

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
