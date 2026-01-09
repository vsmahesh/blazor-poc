using BasicBlazor.Data.Models;
using BasicBlazor.Data.Repositories;
using BasicBlazor.Data.Services;
using FluentAssertions;
using Moq;

namespace BasicBlazor.Tests.Services;

/// <summary>
/// Unit tests for AuthService using Moq to mock IUserRepository.
/// </summary>
public class AuthServiceTests
{
    [Fact]
    public async Task ValidateCredentials_ValidUsernameAndPassword_ReturnsUser()
    {
        // Arrange - Create mock repository and test data
        var mockRepository = new Mock<IUserRepository>();

        var role = new Role { Id = 1, RoleName = "User" };
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            RoleId = 1,
            Role = role
        };

        mockRepository
            .Setup(repo => repo.GetUserByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        var authService = new AuthService(mockRepository.Object);

        // Act - Call with correct credentials
        var result = await authService.ValidateCredentials("testuser", "TestPassword123!");

        // Assert - Verify user is returned with role
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.Role.Should().NotBeNull();
        result.Role!.RoleName.Should().Be("User");
    }

    [Fact]
    public async Task ValidateCredentials_ValidUsernameWrongPassword_ReturnsNull()
    {
        // Arrange - Create user with password hash for different password
        var mockRepository = new Mock<IUserRepository>();

        var role = new Role { Id = 1, RoleName = "User" };
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
            RoleId = 1,
            Role = role
        };

        mockRepository
            .Setup(repo => repo.GetUserByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        var authService = new AuthService(mockRepository.Object);

        // Act - Call with wrong password
        var result = await authService.ValidateCredentials("testuser", "WrongPassword123!");

        // Assert - Verify null is returned
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentials_NonExistentUsername_ReturnsNull()
    {
        // Arrange - Mock repository to return null (user not found)
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .Setup(repo => repo.GetUserByUsernameAsync("nonexistent"))
            .ReturnsAsync((User?)null);

        var authService = new AuthService(mockRepository.Object);

        // Act - Call with non-existent username
        var result = await authService.ValidateCredentials("nonexistent", "AnyPassword123!");

        // Assert - Verify null is returned
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentials_EmptyUsername_ReturnsNull()
    {
        // Arrange - Create mock repository (won't be called)
        var mockRepository = new Mock<IUserRepository>();
        var authService = new AuthService(mockRepository.Object);

        // Act - Call with empty username
        var result = await authService.ValidateCredentials("", "AnyPassword123!");

        // Assert - Verify null is returned
        result.Should().BeNull();

        // Verify repository was never called (early return)
        mockRepository.Verify(
            repo => repo.GetUserByUsernameAsync(It.IsAny<string>()),
            Times.Never
        );
    }
}
