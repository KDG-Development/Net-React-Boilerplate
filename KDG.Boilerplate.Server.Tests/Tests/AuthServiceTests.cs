using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Moq;
using KDG.Boilerplate.Services;
using KDG.UserManagement.Models;
using KDG.UserManagement.Interfaces;
using Newtonsoft.Json;
using KDG.Boilerplate.Server.Models.Users;

namespace KDG.Boilerplate.Server.Tests
{
  public class AuthServiceTests
  {
    private readonly AuthService _authService;
    private readonly string _issuer = "test_issuer";
    private readonly string _audience = "test_audience";
    private readonly string _key = "this_is_a_very_long_secret_key_that_is_at_least_128_bits_long_for_hmacsha256_encryption";

    public AuthServiceTests()
    {
      _authService = new AuthService(_key, _issuer, _audience);
    }

    [Fact]
    public void TokenExpiration_ReturnsCorrectDateTime()
    {
      // Act
      var expiration = AuthService.TokenExpiration();

      // Assert
      Assert.True(expiration > DateTime.UtcNow);
      Assert.True(expiration <= DateTime.UtcNow.AddHours(24));
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsToken()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var user = new User(userId, "test@example.com")
      {
        Permissions = new HashSet<PermissionBase>()
      };

      // Act
      var token = _authService.GenerateToken(user);

      // Assert
      Assert.NotNull(token);
      Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsUser()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var user = new User(userId, "test@example.com")
      {
        Permissions = new HashSet<PermissionBase>()
      };
      
      // Generate a token with the same service instance
      var token = _authService.GenerateToken(user);

      // Act
      var result = _authService.ValidateToken(token);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(userId, result.Id);
      // IUserBase doesn't have an Email property, so we can't check it
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
      // Arrange
      var invalidToken = "invalid_token";

      // Act
      var result = _authService.ValidateToken(invalidToken);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public void TryFindClaimValue_WithExistingClaim_ReturnsValue()
    {
      // Arrange
      var claims = new List<Claim>
      {
        new Claim("test_claim", "test_value")
      };

      // Act
      var result = AuthService.TryFindClaimValue(claims, "test_claim");

      // Assert
      Assert.Equal("test_value", result);
    }

    [Fact]
    public void TryFindClaimValue_WithNonExistingClaim_ReturnsNull()
    {
      // Arrange
      var claims = new List<Claim>();

      // Act
      var result = AuthService.TryFindClaimValue(claims, "non_existing_claim");

      // Assert
      Assert.Null(result);
    }
  }
} 