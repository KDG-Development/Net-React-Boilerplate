using System.IdentityModel.Tokens.Jwt;
using System.Text;
using KDG.Boilerplate.Server.Models.Entities.Users;
using KDG.UserManagement.Models;

namespace KDG.UnitTests.Tests.Auth;

/// <summary>
/// Tests for JWT token security that protect user sessions from hijacking.
/// 
/// Business context: Tokens are the keys to user sessions. Compromised or
/// forged tokens would allow attackers to impersonate users. These tests
/// verify the token system correctly validates authenticity.
/// </summary>
public class TokenSecurityTests
{
    private const string SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    private readonly AuthService _authService = new(SecretKey, Issuer, Audience);

    /// <summary>
    /// Validates that token generation produces valid JWT with user data.
    /// 
    /// Business context: Successful login must produce a valid token containing
    /// user identity. Without this, users cannot use authenticated features.
    /// 
    /// Real-world scenario: User logs in, receives token, can access their
    /// dashboard and place orders.
    /// </summary>
    [Fact]
    public void GenerateToken_ValidUser_ProducesValidJwt()
    {
        var user = CreateTestUser();

        var token = _authService.GenerateToken(user);

        Assert.NotEmpty(token);
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Token has correct structure
        Assert.Equal(3, token.Split('.').Length);
        
        // Token contains user claim
        var userClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user");
        Assert.NotNull(userClaim);
        Assert.Contains(user.Email, userClaim.Value);
        
        // Token has expiration
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }

    /// <summary>
    /// Validates that invalid tokens are rejected.
    /// 
    /// Business context: Random strings, garbage input, and malformed tokens
    /// must not grant access. This prevents brute-force token guessing.
    /// 
    /// Real-world scenario: Attacker sends random strings hoping to find
    /// a valid token. All attempts are rejected.
    /// </summary>
    [Theory]
    [InlineData("invalid.token.here")]
    [InlineData("")]
    public void ValidateToken_InvalidToken_ReturnsNull(string invalidToken)
    {
        var result = _authService.ValidateToken(invalidToken);

        Assert.Null(result);
    }

    /// <summary>
    /// Validates that tokens signed with wrong key are rejected.
    /// 
    /// Business context: Each environment uses its own secret key. Tokens
    /// from staging cannot work in production, and forged tokens are rejected.
    /// 
    /// Real-world scenario: Attacker obtains staging token or creates their
    /// own with different key. Production rejects it.
    /// </summary>
    [Fact]
    public void ValidateToken_WrongKey_ReturnsNull()
    {
        var token = _authService.GenerateToken(CreateTestUser());

        var differentKeyService = new AuthService(
            "completely-different-secret-key-that-wont-match",
            Issuer,
            Audience
        );

        var result = differentKeyService.ValidateToken(token);

        Assert.Null(result);
    }

    /// <summary>
    /// Validates that tokens with wrong issuer or audience are rejected.
    /// 
    /// Business context: Tokens are issued for specific applications. A token
    /// for one app should not work for another with different claims.
    /// 
    /// Real-world scenario: Token issued for admin tool is intercepted and
    /// used against customer API. Wrong audience/issuer, rejected.
    /// </summary>
    [Fact]
    public void ValidateToken_WrongClaims_ReturnsNull()
    {
        var token = _authService.GenerateToken(CreateTestUser());

        // Wrong issuer
        var wrongIssuerService = new AuthService(SecretKey, "wrong-issuer", Audience);
        Assert.Null(wrongIssuerService.ValidateToken(token));

        // Wrong audience
        var wrongAudienceService = new AuthService(SecretKey, Issuer, "wrong-audience");
        Assert.Null(wrongAudienceService.ValidateToken(token));
    }

    /// <summary>
    /// Validates that tampered tokens are rejected.
    /// 
    /// Business context: If someone modifies a token (e.g., changing user ID
    /// to impersonate another user), the signature becomes invalid.
    /// 
    /// Real-world scenario: Attacker intercepts their token and modifies the
    /// user claim to an admin's ID. Signature check fails, attack blocked.
    /// </summary>
    [Fact]
    public void ValidateToken_TamperedPayload_ReturnsNull()
    {
        var token = _authService.GenerateToken(CreateTestUser());

        var parts = token.Split('.');
        var tamperedPayload = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("{\"user\":\"hacked\",\"sub\":\"attacker\"}")
        );
        var tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

        var result = _authService.ValidateToken(tamperedToken);

        Assert.Null(result);
    }

    private static User CreateTestUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        PermissionGroups = new HashSet<PermissionGroupBase>(),
        Permissions = new HashSet<PermissionBase>()
    };
}
