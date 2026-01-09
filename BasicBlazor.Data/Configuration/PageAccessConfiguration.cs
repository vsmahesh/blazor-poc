namespace BasicBlazor.Data.Configuration;

/// <summary>
/// Root configuration model for page access rules.
/// Maps to the root structure of page-access.json.
/// </summary>
public class PageAccessConfiguration
{
    /// <summary>
    /// Gets or sets the list of page access rules.
    /// </summary>
    public List<PageAccessRule> PageAccess { get; set; } = new();
}

/// <summary>
/// Represents a single page access rule defining which roles can access a page.
/// </summary>
public class PageAccessRule
{
    /// <summary>
    /// Gets or sets the page path (e.g., "/page1").
    /// </summary>
    public string PagePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the list of role names allowed to access this page.
    /// </summary>
    public List<string> AllowedRoles { get; set; } = new();

    /// <summary>
    /// Gets or sets the display name for navigation menus (e.g., "Page 1").
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Gets or sets the order for sorting and determining default landing pages.
    /// Lower order values are shown first.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the optional Bootstrap icon class name (e.g., "bi-star-fill").
    /// The "bi" prefix is added automatically in the navigation menu.
    /// If not provided, a default icon will be used.
    /// </summary>
    public string? Icon { get; set; }
}
