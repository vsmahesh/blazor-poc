namespace BasicBlazor.Extension.ClientA.Services;

/// <summary>
/// Service for managing featured products for ClientA.
/// Provides client-specific business logic for product listings.
/// </summary>
public class FeaturedProductsService
{
    // Hardcoded product list for demonstration purposes
    // In a real application, this could:
    // - Query from database using injected AppDbContext
    // - Load from configuration file
    // - Fetch from external API
    private readonly List<string> _featuredProducts = new()
    {
        "Premium Widget Pro",
        "Deluxe Gadget Plus",
        "Ultimate Tool Kit",
        "Professional Accessory Set",
        "Advanced Component Bundle",
        "Elite Feature Pack"
    };

    /// <summary>
    /// Gets the list of featured products for ClientA.
    /// </summary>
    /// <returns>List of product names.</returns>
    public List<string> GetFeaturedProducts()
    {
        return _featuredProducts;
    }

    // Example of how to use shared services (AppDbContext, etc.):
    // private readonly AppDbContext _dbContext;
    //
    // public FeaturedProductsService(AppDbContext dbContext)
    // {
    //     _dbContext = dbContext;
    // }
    //
    // public async Task<List<Product>> GetFeaturedProductsFromDatabaseAsync()
    // {
    //     return await _dbContext.Products
    //         .Where(p => p.IsFeatured)
    //         .ToListAsync();
    // }
}
