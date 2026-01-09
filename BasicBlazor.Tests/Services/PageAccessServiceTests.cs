using BasicBlazor.Data.Configuration;
using BasicBlazor.Data.Services;
using FluentAssertions;

namespace BasicBlazor.Tests.Services;

/// <summary>
/// Unit tests for PageAccessService.
/// Tests role-based page access authorization logic.
/// </summary>
public class PageAccessServiceTests
{
    /// <summary>
    /// Creates a PageAccessService instance with test configuration matching page-access.json.
    /// </summary>
    private PageAccessService CreateTestService()
    {
        var config = new PageAccessConfiguration
        {
            PageAccess = new List<PageAccessRule>
            {
                new() { PagePath = "/page1", AllowedRoles = new List<string> { "User" }, DisplayName = "Page 1", Order = 1 },
                new() { PagePath = "/page2", AllowedRoles = new List<string> { "Manager" }, DisplayName = "Page 2", Order = 2 },
                new() { PagePath = "/page3", AllowedRoles = new List<string> { "User", "Manager" }, DisplayName = "Page 3", Order = 3 },
                new() { PagePath = "/page4", AllowedRoles = new List<string> { "User", "Manager", "Admin" }, DisplayName = "Page 4", Order = 4 }
            }
        };

        return new PageAccessService(config);
    }

    [Fact]
    public void IsPageAccessible_UserRolePage1_ReturnsTrue()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Check if User role can access Page1
        var result = service.IsPageAccessible("/page1", "User");

        // Assert - User role should have access to Page1
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPageAccessible_UserRolePage2_ReturnsFalse()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Check if User role can access Page2 (Manager only)
        var result = service.IsPageAccessible("/page2", "User");

        // Assert - User role should NOT have access to Page2
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPageAccessible_ManagerRolePage3_ReturnsTrue()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Check if Manager role can access Page3 (User + Manager)
        var result = service.IsPageAccessible("/page3", "Manager");

        // Assert - Manager role should have access to Page3
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAllowedPagesForRole_UserRole_ReturnsCorrectPages()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Get allowed pages for User role
        var result = service.GetAllowedPagesForRole("User");

        // Assert - User should have access to Page1, Page3, Page4 (in order)
        result.Should().HaveCount(3);
        result[0].PagePath.Should().Be("/page1");
        result[0].Order.Should().Be(1);
        result[1].PagePath.Should().Be("/page3");
        result[1].Order.Should().Be(3);
        result[2].PagePath.Should().Be("/page4");
        result[2].Order.Should().Be(4);
    }

    [Fact]
    public void GetAllowedPagesForRole_ManagerRole_ReturnsCorrectPages()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Get allowed pages for Manager role
        var result = service.GetAllowedPagesForRole("Manager");

        // Assert - Manager should have access to Page2, Page3, Page4 (in order)
        result.Should().HaveCount(3);
        result[0].PagePath.Should().Be("/page2");
        result[0].Order.Should().Be(2);
        result[1].PagePath.Should().Be("/page3");
        result[1].Order.Should().Be(3);
        result[2].PagePath.Should().Be("/page4");
        result[2].Order.Should().Be(4);
    }

    [Fact]
    public void GetAllowedPagesForRole_AdminRole_ReturnsCorrectPages()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Get allowed pages for Admin role
        var result = service.GetAllowedPagesForRole("Admin");

        // Assert - Admin should have access to Page4 only
        result.Should().HaveCount(1);
        result[0].PagePath.Should().Be("/page4");
        result[0].Order.Should().Be(4);
        result[0].DisplayName.Should().Be("Page 4");
    }

    [Fact]
    public void GetFirstAllowedPage_UserRole_ReturnsPage1()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Get first allowed page for User role
        var result = service.GetFirstAllowedPage("User");

        // Assert - User's first allowed page should be Page1 (lowest order)
        result.Should().Be("/page1");
    }

    [Fact]
    public void GetFirstAllowedPage_ManagerRole_ReturnsPage2()
    {
        // Arrange - Create service with test configuration
        var service = CreateTestService();

        // Act - Get first allowed page for Manager role
        var result = service.GetFirstAllowedPage("Manager");

        // Assert - Manager's first allowed page should be Page2 (lowest order for Manager)
        result.Should().Be("/page2");
    }
}
