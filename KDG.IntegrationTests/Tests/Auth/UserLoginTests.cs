using Dapper;
using KDG.Boilerplate.Server.Models.Requests.Auth;
using KDG.Boilerplate.Services;
using KDG.IntegrationTests.Infrastructure;

namespace KDG.IntegrationTests.Tests.Auth;

/// <summary>
/// Integration tests for user login flow that verify credentials and profile
/// loading work correctly with a real database.
/// 
/// Business context: Login is the gateway to all authenticated features.
/// Broken login blocks customers from placing orders and admins from
/// managing the platform.
/// </summary>
public class UserLoginTests : IntegrationTestBase
{
    public UserLoginTests(DatabaseTestFixture fixture) : base(fixture) { }

    #region Test Data Setup

    private async Task CleanupTestData()
    {
        using var connection = await GetDatabaseConnection();
        await connection.ExecuteAsync("DELETE FROM user_permission_groups WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@test.example.com')");
        await connection.ExecuteAsync("DELETE FROM user_passwords WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@test.example.com')");
        await connection.ExecuteAsync("DELETE FROM users WHERE email LIKE '%@test.example.com'");
        await connection.ExecuteAsync("DELETE FROM organizations WHERE name LIKE 'Test Org%'");
    }

    #endregion

    /// <summary>
    /// Verifies that a user with valid credentials receives their full profile.
    /// 
    /// Business context: Organization membership determines pricing tiers and
    /// permissions control feature access. Both must load correctly.
    /// 
    /// Real-world scenario: Sales rep from "Acme Corp" logs in and sees their
    /// company's negotiated pricing and admin dashboard access.
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ReturnsFullProfile()
    {
        await CleanupTestData();
        
        var orgId = await TestData.Organizations.Create("Test Org Alpha");
        var email = "salesrep@test.example.com";
        var password = "SecurePassword123!";
        var userId = await TestData.Users.Create(email, password, orgId);
        await TestData.Users.AssignPermissionGroup(userId, "admin");

        var userService = GetService<IUserService>();
        var request = new LoginRequest { Email = email, Password = password };

        var user = await userService.UserLoginAsync(request);

        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.NotNull(user.Organization);
        Assert.Equal("Test Org Alpha", user.Organization.Name);
        Assert.Contains(user.PermissionGroups, pg => pg.PermissionGroup == "admin");
    }

    /// <summary>
    /// Verifies that invalid credentials return null without information leakage.
    /// 
    /// Business context: Failed logins must fail gracefully. Same response for
    /// wrong password and non-existent email prevents account enumeration.
    /// 
    /// Real-world scenario: User mistypes password or attacker tries random
    /// emails. Both get generic "invalid credentials" response.
    /// </summary>
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsNull()
    {
        await CleanupTestData();
        
        var email = "wrongpass@test.example.com";
        await TestData.Users.Create(email, "CorrectPassword123!");

        var userService = GetService<IUserService>();
        
        // Wrong password
        var wrongPassResult = await userService.UserLoginAsync(
            new LoginRequest { Email = email, Password = "WrongPassword456!" });
        Assert.Null(wrongPassResult);

        // Non-existent email
        var noUserResult = await userService.UserLoginAsync(
            new LoginRequest { Email = "nonexistent@test.example.com", Password = "AnyPass!" });
        Assert.Null(noUserResult);
    }

    /// <summary>
    /// Verifies that user without organization can still log in.
    /// 
    /// Business context: Website administrators operate outside of organizations.
    /// All other users are required to have an organization - a user without one
    /// would indicate a data integrity issue.
    /// 
    /// Real-world scenario: Platform admin logs in to manage site-wide settings.
    /// They have no organization because they're not a B2B customer.
    /// </summary>
    [Fact]
    public async Task Login_UserWithoutOrganization_ReturnsUserWithNullOrg()
    {
        await CleanupTestData();
        
        var email = "noorg@test.example.com";
        var password = "NoOrgPass789!";
        await TestData.Users.Create(email, password, organizationId: null);

        var userService = GetService<IUserService>();
        var request = new LoginRequest { Email = email, Password = password };

        var user = await userService.UserLoginAsync(request);

        Assert.NotNull(user);
        Assert.Null(user.Organization);
    }
}
