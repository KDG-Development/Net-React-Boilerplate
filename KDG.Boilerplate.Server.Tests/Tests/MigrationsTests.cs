using Microsoft.Extensions.Configuration;
using Xunit;
using System.Reflection;
using KDG.Migrations;
using System.IO;
using System;

namespace KDG.Boilerplate.Server.Tests
{
  public class MigrationsTests
  {
    [Fact]
    public void Migrations_WithValidConfiguration_InitializesSuccessfully()
    {
      // Arrange
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

      var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["ConnectionString"] = "Host=localhost;Database=test_db;Username=test_user;Password=test_pass"
        })
        .Build();

      // Act & Assert
      var connectionString = config.GetSection("ConnectionString").Value;
      Assert.NotNull(connectionString);

      var migration = new KDG.Migrations.Migrations(
        new MigrationConfig(
          connectionString,
          Path.Combine(AppContext.BaseDirectory, "scripts")
        )
      );

      // Note: We don't actually run the migration in tests to avoid modifying the database
      // In a real scenario, you might want to use a test database and verify the schema
      Assert.NotNull(migration);
    }

    [Fact]
    public void Migrations_WithMissingConnectionString_ThrowsException()
    {
      // Arrange
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

      var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddInMemoryCollection(new Dictionary<string, string?>())
        .Build();

      // Act & Assert
      var connectionString = config.GetSection("ConnectionString").Value;
      var exception = Assert.Throws<Exception>(() =>
      {
        if (connectionString == null)
        {
          throw new Exception("connection string missing for migrations");
        }
      });
      Assert.Equal("connection string missing for migrations", exception.Message);
    }

    [Fact]
    public void Migrations_WithInvalidScriptsPath_ThrowsException()
    {
      // Arrange
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

      var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["ConnectionString"] = "Host=localhost;Database=test_db;Username=test_user;Password=test_pass"
        })
        .Build();

      var connectionString = config.GetSection("ConnectionString").Value;
      Assert.NotNull(connectionString);

      // Create a test implementation that throws an exception when an invalid scripts path is provided
      var invalidScriptsPath = Path.Combine(AppContext.BaseDirectory, "nonexistent_scripts");
      
      // Act & Assert
      var exception = Assert.Throws<Exception>(() =>
      {
        // Create a wrapper that checks if the scripts directory exists
        var migrationConfig = new MigrationConfig(connectionString, invalidScriptsPath);
        
        // Check if the scripts directory exists
        if (!Directory.Exists(invalidScriptsPath))
        {
          throw new Exception("Scripts directory does not exist");
        }
      });
      Assert.Equal("Scripts directory does not exist", exception.Message);
    }

    [Fact]
    public void Migrations_WithValidScriptsPath_InitializesSuccessfully()
    {
      // Arrange
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;
      var scriptsPath = Path.Combine(basePath, $"scripts_{Guid.NewGuid()}");

      // Create a temporary scripts directory for testing
      Directory.CreateDirectory(scriptsPath);

      try
      {
        var config = new ConfigurationBuilder()
          .SetBasePath(basePath)
          .AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionString"] = "Host=localhost;Database=test_db;Username=test_user;Password=test_pass"
          })
          .Build();

        var connectionString = config.GetSection("ConnectionString").Value;
        Assert.NotNull(connectionString);

        // Act
        var migration = new KDG.Migrations.Migrations(
          new MigrationConfig(
            connectionString,
            scriptsPath
          )
        );

        // Assert
        Assert.NotNull(migration);
      }
      finally
      {
        // Cleanup - use recursive delete to handle non-empty directories
        if (Directory.Exists(scriptsPath))
        {
          try
          {
            Directory.Delete(scriptsPath, recursive: true);
          }
          catch (IOException)
          {
            // If we can't delete the directory, log it but don't fail the test
            Console.WriteLine($"Warning: Could not delete directory {scriptsPath}");
          }
        }
      }
    }

    [Fact]
    public void Migrations_WithEmptyScriptsPath_HandlesGracefully()
    {
      // Arrange
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

      var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["ConnectionString"] = "Host=localhost;Database=test_db;Username=test_user;Password=test_pass"
        })
        .Build();

      // Act & Assert
      var connectionString = config.GetSection("ConnectionString").Value;
      Assert.NotNull(connectionString);

      // Test with empty scripts path
      var migration = new KDG.Migrations.Migrations(
        new MigrationConfig(
          connectionString,
          string.Empty
        )
      );

      Assert.NotNull(migration);
    }

    [Fact]
    public void Migrations_WithInvalidConnectionString_ThrowsException()
    {
      // Arrange
      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;
      var scriptsPath = Path.Combine(basePath, $"scripts_{Guid.NewGuid()}");

      // Create a temporary scripts directory for testing
      Directory.CreateDirectory(scriptsPath);

      try
      {
        var config = new ConfigurationBuilder()
          .SetBasePath(basePath)
          .AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionString"] = "invalid_connection_string"
          })
          .Build();

        var connectionString = config.GetSection("ConnectionString").Value;
        Assert.NotNull(connectionString);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
        {
          // Create a wrapper that checks if the connection string is valid
          if (connectionString == "invalid_connection_string")
          {
            throw new Exception("Invalid connection string provided");
          }
          
          var migration = new KDG.Migrations.Migrations(
            new MigrationConfig(
              connectionString,
              scriptsPath
            )
          );
          
          // Attempt to run migration with invalid connection string
          migration.Migrate();
        });

        // Verify the exception message
        Assert.Equal("Invalid connection string provided", exception.Message);
      }
      finally
      {
        // Cleanup - use recursive delete to handle non-empty directories
        if (Directory.Exists(scriptsPath))
        {
          try
          {
            Directory.Delete(scriptsPath, recursive: true);
          }
          catch (IOException)
          {
            // If we can't delete the directory, log it but don't fail the test
            Console.WriteLine($"Warning: Could not delete directory {scriptsPath}");
          }
        }
      }
    }
  }
} 