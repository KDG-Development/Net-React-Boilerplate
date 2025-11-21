using System.Data;
using Dapper;
using DotNet.Testcontainers.Builders;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace KDG.IntegrationTests.Infrastructure;

public class DatabaseTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Determine database source and connection string
        ConnectionString = await DetermineConnectionString();
        
        // Run database migrations
        await RunMigrations();
    }

    public async Task DisposeAsync()
    {
        // Stop and dispose Testcontainer
        if (_postgresContainer != null) {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }
    
    private async Task<string> DetermineConnectionString()
    {
        Console.WriteLine("Starting Testcontainers PostgreSQL...");
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("kdg-integration-test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
        
        await _postgresContainer.StartAsync();
        
        Console.WriteLine("Testcontainers PostgreSQL started successfully");
        
        // Determine connection string based on environment:
        // - Docker / Azure: Use container IP when ASPNETCORE_ENVIRONMENT=Test (Docker scenario)
        // - Local dotnet test: Use GetConnectionString() which returns localhost with mapped port
        var useContainerIp = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
        
        if (useContainerIp) {
            // Running in Docker - use container IP for direct container-to-container communication
            var containerIp = _postgresContainer.IpAddress;
            var connectionString = $"Host={containerIp};Port=5432;Database=kdg-integration-test;Username=postgres;Password=postgres";
            Console.WriteLine($"Using container IP connection string: Host={containerIp};Port=5432");
            return connectionString;
        } else {
            // Running locally - use GetConnectionString() which returns localhost with mapped port
            var connectionString = _postgresContainer.GetConnectionString();
            Console.WriteLine($"Using local connection string: {connectionString}");
            return connectionString;
        }
    }
    
    private Task RunMigrations()
    {
        Console.WriteLine("Running database migrations...");
        
        var scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts");
        if (!Directory.Exists(scriptsPath)) {
            throw new InvalidOperationException($"Migration scripts directory not found at: {scriptsPath}");
        }
        
        var migrationConfig = new KDG.Migrations.MigrationConfig(
            KDG.Migrations.DatabaseType.PostgreSQL, 
            ConnectionString, 
            scriptsPath
        );
        var migration = new KDG.Migrations.Migrations(migrationConfig);
        migration.Migrate();
        
        Console.WriteLine("Database migrations completed successfully");
        return Task.CompletedTask;
    }
}

