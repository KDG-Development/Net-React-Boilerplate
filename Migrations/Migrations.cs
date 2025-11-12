using KDG.Migrations;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var assemblyLocation = Assembly.GetExecutingAssembly().Location;
var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

string? connectionString = null;

// Check for CONNECTION_STRING environment variable first (for test scenarios)
var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (!string.IsNullOrEmpty(envConnectionString)) {
    connectionString = envConnectionString;
} else {
    // Fall back to config file (for non-test usage)
    IConfiguration config =
        new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    connectionString = config.GetSection("ConnectionString").Value;
}

if (connectionString == null){
    throw new Exception("connection string missing for migrations");
}

KDG.Migrations.Migrations migration = new KDG.Migrations.Migrations(
    new MigrationConfig(
        KDG.Migrations.DatabaseType.PostgreSQL,
        connectionString,
        Path.Combine(AppContext.BaseDirectory, "scripts")
    )
);
return migration.Migrate();