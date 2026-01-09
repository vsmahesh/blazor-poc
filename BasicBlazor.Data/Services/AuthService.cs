using BasicBlazor.Data.Models;
using BasicBlazor.Data.Repositories;

namespace BasicBlazor.Data.Services;

/// <summary>
/// Service for authenticating users by validating credentials.
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the AuthService class.
    /// </summary>
    /// <param name="userRepository">The user repository for accessing user data.</param>
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Validates user credentials by verifying username and password.
    /// Returns null for any authentication failure to prevent username enumeration attacks.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>The User object with Role if credentials are valid; otherwise, null.</returns>
    public async Task<User?> ValidateCredentials(string username, string password)
    {
        // Return null for empty/null username (defensive check)
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        // Retrieve user from repository (includes Role via eager loading)
        var user = await _userRepository.GetUserByUsernameAsync(username);

        // Return null if user not found
        if (user == null)
        {
            return null;
        }

        // Verify password using BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        // Return user if password is valid, otherwise null
        return isPasswordValid ? user : null;
    }
}
