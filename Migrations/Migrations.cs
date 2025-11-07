using KDG.Migrations;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var assemblyLocation = Assembly.GetExecutingAssembly().Location;
var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

IConfiguration config =
    new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetSection("ConnectionString").Value;

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