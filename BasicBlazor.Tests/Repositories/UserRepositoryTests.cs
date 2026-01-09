using BasicBlazor.Data.Data;
using BasicBlazor.Data.Models;
using BasicBlazor.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BasicBlazor.Tests.Repositories;

/// <summary>
/// Unit tests for UserRepository using EF Core InMemory provider.
/// </summary>
public class UserRepositoryTests
{
    /// <summary>
    /// Creates a new in-memory database context for testing.
    /// Each test uses a unique database name to prevent cross-test pollution.
    /// </summary>
    private AppDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser()
    {
        // Arrange - Create in-memory database and seed test data
        using var context = CreateInMemoryContext("GetUserByUsernameAsync_ExistingUser");

        var role = new Role { Id = 1, RoleName = "User" };
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "hashedpassword123",
            RoleId = 1
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        // Act - Call the repository method
        var result = await repository.GetUserByUsernameAsync("testuser");

        // Assert - Verify user is returned with correct data
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.PasswordHash.Should().Be("hashedpassword123");
        result.RoleId.Should().Be(1);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange - Create in-memory database with test users
        using var context = CreateInMemoryContext("GetUserByUsernameAsync_NonExistentUser");

        var role = new Role { Id = 1, RoleName = "User" };
        var user = new User
        {
            Id = 1,
            Username = "existinguser",
            PasswordHash = "hashedpassword123",
            RoleId = 1
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        // Act - Search for non-existent user
        var result = await repository.GetUserByUsernameAsync("nonexistentuser");

        // Assert - Verify null is returned
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUsernameAsync_IncludesRoleNavigation()
    {
        // Arrange - Create in-memory database with user and role
        using var context = CreateInMemoryContext("GetUserByUsernameAsync_IncludesRole");

        var role = new Role { Id = 1, RoleName = "Manager" };
        var user = new User
        {
            Id = 1,
            Username = "manageruser",
            PasswordHash = "hashedpassword123",
            RoleId = 1
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        // Act - Retrieve user
        var result = await repository.GetUserByUsernameAsync("manageruser");

        // Assert - Verify Role navigation property is loaded
        result.Should().NotBeNull();
        result!.Role.Should().NotBeNull();
        result.Role!.RoleName.Should().Be("Manager");
    }
}
