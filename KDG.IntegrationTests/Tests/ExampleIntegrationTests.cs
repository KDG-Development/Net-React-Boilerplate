using Dapper;
using KDG.IntegrationTests.Infrastructure;
using Xunit;

namespace KDG.IntegrationTests.Tests;

public class ExampleIntegrationTests : IntegrationTestBase
{
    public ExampleIntegrationTests(DatabaseTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldConnectToDatabase()
    {
        // Arrange & Act
        using var connection = await GetDatabaseConnection();
        
        // Assert
        Assert.NotNull(connection);
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task ShouldHaveMigrationsApplied()
    {
        // Arrange & Act
        using var connection = await GetDatabaseConnection();
        var tableExists = await connection.QuerySingleOrDefaultAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_name = 'users')");
        
        // Assert
        Assert.True(tableExists, "Users table should exist after migrations");
    }

    [Fact]
    public void ShouldCreateAuthController()
    {
        // Arrange & Act
        var controller = CreateAuthController();
        
        // Assert
        Assert.NotNull(controller);
    }
}

