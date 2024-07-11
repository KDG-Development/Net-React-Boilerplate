using KDG.Migrations;
using Microsoft.Extensions.Configuration;

IConfiguration config =
    new ConfigurationBuilder()
    // .AddJsonFile("appsettings.json") // example only
    .AddJsonFile("appsettings.development.json")
    .Build();


var connectionString = config.GetSection("ConnectionString").Value;

Console.WriteLine($"conn: {connectionString}");

if (connectionString == null){
    throw new Exception("connection string missing for migrations");
}

KDG.Migrations.Migrations migration = new KDG.Migrations.Migrations(
    new MigrationConfig(
        connectionString,
        "scripts"
    )
);
migration.Migrate();