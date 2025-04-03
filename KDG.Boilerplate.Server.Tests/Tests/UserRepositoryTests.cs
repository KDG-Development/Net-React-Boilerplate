using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using KDG.Boilerplate.Models.DTO;
using KDG.Database.Interfaces;
using KDG.Boilerplate.Server.Models.Users;
using KDG.Boilerplate.Server.Models.ActiveRecords;
using KDG.Boilerplate.Services;
using Dapper;
using Npgsql;
using KDG.UserManagement.Models;

namespace KDG.Boilerplate.Server.Tests
{
  public class UserRepositoryTests
  {
    private readonly Mock<IDatabase<NpgsqlConnection, NpgsqlTransaction>> _mockDatabase;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
      _mockDatabase = new Mock<IDatabase<NpgsqlConnection, NpgsqlTransaction>>();
      _userRepository = new UserRepository(_mockDatabase.Object);
    }

    [Fact]
    public async Task UserLogin_WithValidCredentials_ReturnsUser()
    {
      // Arrange
      var authPayload = new UserAuth("test@example.com", "password");
      var userActiveRecord = new UserActiveRecord
      {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        PermissionGroups = new[] { "admin" },
        Permissions = new[] { "read", "write" }
      };

      var expectedUser = new User(userActiveRecord.Id, userActiveRecord.Email)
      {
        PermissionGroups = new HashSet<PermissionGroupBase>(
          userActiveRecord.PermissionGroups.Select(g => new PermissionGroupBase(g))
        ),
        Permissions = new HashSet<PermissionBase>(
          userActiveRecord.Permissions.Select(p => new PermissionBase(p))
        )
      };

      _mockDatabase.Setup(d => d.withConnection(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
        .Returns<Func<NpgsqlConnection, Task<User>>>(func => Task.FromResult(expectedUser));

      // Act
      var result = await _userRepository.UserLogin(authPayload);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(userActiveRecord.Id, result.Id);
      Assert.Equal(userActiveRecord.Email, result.Email);
      Assert.Equal(userActiveRecord.PermissionGroups.Length, result.PermissionGroups.Count);
      Assert.Equal(userActiveRecord.Permissions.Length, result.Permissions.Count);
    }

    [Fact]
    public async Task UserLogin_WithInvalidCredentials_ReturnsNull()
    {
      // Arrange
      var authPayload = new UserAuth("nonexistent@example.com", "password");

      _mockDatabase.Setup(d => d.withConnection(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
        .Returns<Func<NpgsqlConnection, Task<User>>>(func => Task.FromResult<User>(null!));

      // Act
      var result = await _userRepository.UserLogin(authPayload);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public async Task UserLogin_WithNoPermissions_ReturnsUserWithoutPermissions()
    {
      // Arrange
      var authPayload = new UserAuth("test@example.com", "password");
      var userActiveRecord = new UserActiveRecord
      {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        PermissionGroups = Array.Empty<string>(),
        Permissions = Array.Empty<string>()
      };

      var expectedUser = new User(userActiveRecord.Id, userActiveRecord.Email)
      {
        PermissionGroups = new HashSet<PermissionGroupBase>(),
        Permissions = new HashSet<PermissionBase>()
      };

      _mockDatabase.Setup(d => d.withConnection(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
        .Returns<Func<NpgsqlConnection, Task<User>>>(func => Task.FromResult(expectedUser));

      // Act
      var result = await _userRepository.UserLogin(authPayload);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(userActiveRecord.Id, result.Id);
      Assert.Equal(userActiveRecord.Email, result.Email);
      Assert.Empty(result.PermissionGroups);
      Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task UserLogin_WithNullPermissionGroups_HandlesGracefully()
    {
      // Arrange
      var authPayload = new UserAuth("test@example.com", "password");
      var userActiveRecord = new UserActiveRecord
      {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        PermissionGroups = null!,
        Permissions = new[] { "read" }
      };

      var expectedUser = new User(userActiveRecord.Id, userActiveRecord.Email)
      {
        PermissionGroups = new HashSet<PermissionGroupBase>(),
        Permissions = new HashSet<PermissionBase>(
          userActiveRecord.Permissions.Select(p => new PermissionBase(p))
        )
      };

      _mockDatabase.Setup(d => d.withConnection(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
        .Returns<Func<NpgsqlConnection, Task<User>>>(func => Task.FromResult(expectedUser));

      // Act
      var result = await _userRepository.UserLogin(authPayload);

      // Assert
      Assert.NotNull(result);
      Assert.Empty(result.PermissionGroups);
      Assert.Single(result.Permissions);
    }

    [Fact]
    public async Task UserLogin_WithNullPermissions_HandlesGracefully()
    {
      // Arrange
      var authPayload = new UserAuth("test@example.com", "password");
      var userActiveRecord = new UserActiveRecord
      {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        PermissionGroups = new[] { "admin" },
        Permissions = null!
      };

      var expectedUser = new User(userActiveRecord.Id, userActiveRecord.Email)
      {
        PermissionGroups = new HashSet<PermissionGroupBase>(
          userActiveRecord.PermissionGroups.Select(g => new PermissionGroupBase(g))
        ),
        Permissions = new HashSet<PermissionBase>()
      };

      _mockDatabase.Setup(d => d.withConnection(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
        .Returns<Func<NpgsqlConnection, Task<User>>>(func => Task.FromResult(expectedUser));

      // Act
      var result = await _userRepository.UserLogin(authPayload);

      // Assert
      Assert.NotNull(result);
      Assert.Single(result.PermissionGroups);
      Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task UserLogin_WithDatabaseError_PropagatesException()
    {
      // Arrange
      var authPayload = new UserAuth("test@example.com", "password");

      _mockDatabase.Setup(d => d.withConnection(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
        .ThrowsAsync(new Exception("Database error"));

      // Act & Assert
      await Assert.ThrowsAsync<Exception>(() => _userRepository.UserLogin(authPayload));
    }
  }
} 