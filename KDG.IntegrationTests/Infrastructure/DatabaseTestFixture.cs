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
    private bool _usingTestcontainers;
    
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
        // Stop and dispose Testcontainer if we created one
        if (_usingTestcontainers && _postgresContainer != null) {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }
    
    private async Task<string> DetermineConnectionString()
    {
        // Priority 1: Check if running in CI with service container
        if (IsRunningInCI()) {
            var serviceContainerConnectionString = await TryGetServiceContainerConnectionString();
            if (serviceContainerConnectionString != null) {
                Console.WriteLine("Using Azure Pipelines service container");
                _usingTestcontainers = false;
                return serviceContainerConnectionString;
            }
        }
        
        // Priority 2: Check for Docker Compose setup
        var dockerComposeConnectionString = TryGetDockerComposeConnectionString();
        if (dockerComposeConnectionString != null) {
            Console.WriteLine("Using Docker Compose database");
            _usingTestcontainers = false;
            return dockerComposeConnectionString;
        }
        
        // Priority 3: Start Testcontainers
        Console.WriteLine("Starting Testcontainers PostgreSQL...");
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("kdg-integration-test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
        
        await _postgresContainer.StartAsync();
        _usingTestcontainers = true;
        
        Console.WriteLine("Testcontainers PostgreSQL started successfully");
        return _postgresContainer.GetConnectionString();
    }
    
    private bool IsRunningInCI()
    {
        // Azure Pipelines environment variables
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_ID")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD"));
    }
    
    private async Task<string?> TryGetServiceContainerConnectionString()
    {
        // In Azure Pipelines, service containers are available at localhost:5432
        var connectionString = "Host=localhost;Port=5432;Database=kdg-integration-test;Username=postgres;Password=postgres";
        
        try {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return connectionString;
        } catch {
            return null;
        }
    }
    
    private string? TryGetDockerComposeConnectionString()
    {
        // Docker Compose integration-test-db service connection string
        var connectionString = "Host=integration-test-db;Port=5432;Database=kdg-integration-test;Username=postgres;Password=postgres";
        
        try {
            // Test if the connection works
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            connection.Close();
            return connectionString;
        } catch {
            return null;
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

