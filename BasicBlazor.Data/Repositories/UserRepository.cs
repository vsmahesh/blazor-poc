using BasicBlazor.Data.Data;
using BasicBlazor.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BasicBlazor.Data.Repositories;

/// <summary>
/// Repository implementation for User entity data access using Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserRepository class.
    /// </summary>
    /// <param name="context">The database context for accessing user data.</param>
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a user by username with the associated Role navigation property.
    /// Uses eager loading to prevent N+1 query issues.
    /// </summary>
    /// <param name="username">The username to search for (case-sensitive).</param>
    /// <returns>The User object with Role if found; otherwise, null.</returns>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}
