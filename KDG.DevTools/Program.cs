using System.Reflection;
using KDG.Database;
using KDG.DevTools.Seeders;
using Microsoft.Extensions.Configuration;

// Environment guard - only allow running in Development
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (env != "Development")
{
    Console.Error.WriteLine("ERROR: DevTools can only run in Development environment.");
    Console.Error.WriteLine($"Current environment: {env ?? "(not set)"}");
    Console.Error.WriteLine("Set ASPNETCORE_ENVIRONMENT=Development to use this tool.");
    return 1;
}

// Load configuration
var assemblyLocation = Assembly.GetExecutingAssembly().Location;
var basePath = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config["ConnectionString"]
    ?? throw new Exception("ConnectionString not found in appsettings.json");

// Parse command line arguments
if (args.Length == 0 || args[0] != "seed")
{
    PrintUsage();
    return 1;
}

var categoryCount = 10;
var productCount = 50;
var seedCategories = false;
var seedProducts = false;
var seedUser = false;

for (var i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--all":
            seedCategories = true;
            seedProducts = true;
            break;
        case "--categories":
            seedCategories = true;
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var catCount))
            {
                categoryCount = catCount;
                i++;
            }
            break;
        case "--products":
            seedProducts = true;
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var prodCount))
            {
                productCount = prodCount;
                i++;
            }
            break;
        case "--user":
            seedUser = true;
            break;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            PrintUsage();
            return 1;
    }
}

if (!seedCategories && !seedProducts && !seedUser)
{
    Console.Error.WriteLine("No seeders specified. Use --all, --categories, --products, or --user.");
    PrintUsage();
    return 1;
}

// Initialize database connection
var database = new PostgreSQL(connectionString);

Console.WriteLine("=== KDG DevTools - Data Seeder ===");
Console.WriteLine($"Environment: {env}");
Console.WriteLine();

try
{
    if (seedCategories)
    {
        Console.WriteLine($"Seeding {categoryCount} categories...");
        var categorySeeder = new CategorySeeder(database);
        var created = await categorySeeder.SeedAsync(categoryCount);
        Console.WriteLine($"Created {created} categories.");
        Console.WriteLine();
    }

    if (seedProducts)
    {
        Console.WriteLine($"Seeding {productCount} products...");
        var productSeeder = new ProductSeeder(database);
        var created = await productSeeder.SeedAsync(productCount);
        Console.WriteLine($"Created {created} products.");
        Console.WriteLine();
    }

    if (seedUser)
    {
        var userSeeder = new UserSeeder(database);
        await userSeeder.SeedAsync();
        Console.WriteLine();
    }

    Console.WriteLine("Seeding completed successfully!");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error during seeding: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

void PrintUsage()
{
    Console.WriteLine(@"
KDG DevTools - Data Seeder

Usage:
  docker compose run --rm devtools seed [options]

Options:
  --all                 Seed all data types with default counts
  --categories [count]  Seed categories (default: 10)
  --products [count]    Seed products (default: 50)
  --user                Seed a user (interactive prompts)

Examples:
  docker compose run --rm devtools seed --all
  docker compose run --rm devtools seed --categories 20 --products 100
  docker compose run --rm devtools seed --categories
  docker compose run --rm devtools seed --user
");
}

