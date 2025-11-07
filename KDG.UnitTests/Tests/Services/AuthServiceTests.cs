using KDG.Boilerplate.Server.Models.Users;
using KDG.UserManagement.Interfaces;
using KDG.UserManagement.Models;
using Xunit;

namespace KDG.UnitTests.Tests.Services;

public class AuthServiceTests
{
    private readonly string _secretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm";
    private readonly string _issuer = "test-issuer";
    private readonly string _audience = "test-audience";

    [Fact]
    public void GenerateToken_ValidUser_ReturnsToken()
    {
        var user = new User {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PermissionGroups = new HashSet<PermissionGroupBase>(),
            Permissions = new HashSet<PermissionBase>()
        };
        var authService = new AuthService(_secretKey, _issuer, _audience);

        var token = authService.GenerateToken(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        var authService = new AuthService(_secretKey, _issuer, _audience);
        var invalidToken = "invalid.token.here";

        var validatedUser = authService.ValidateToken(invalidToken);

        Assert.Null(validatedUser);
    }
}

