using BasicBlazor.Data.Models;

namespace BasicBlazor.Data.Repositories;

/// <summary>
/// Repository interface for User entity data access operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by username for authentication purposes.
    /// Includes the associated Role navigation property.
    /// </summary>
    /// <param name="username">The username to search for (case-sensitive).</param>
    /// <returns>The User object with Role if found; otherwise, null.</returns>
    Task<User?> GetUserByUsernameAsync(string username);
}
