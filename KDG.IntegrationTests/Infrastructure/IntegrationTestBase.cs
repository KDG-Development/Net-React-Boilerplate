using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using KDG.Boilerplate.Server.Controllers;
using KDG.Boilerplate.Services;
using KDG.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace KDG.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<DatabaseTestFixture>
{
    protected readonly DatabaseTestFixture DatabaseFixture;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IConfiguration Configuration;

    protected IntegrationTestBase(DatabaseTestFixture databaseFixture)
    {
        DatabaseFixture = databaseFixture;
        Configuration = BuildConfiguration();
        ServiceProvider = SetupServices();
    }

    private IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Default JWT settings (can be overridden by config files)
                ["Jwt:Key"] = "test-jwt-key-for-integration-tests",
                ["Jwt:Issuer"] = "kdg-boilerplate-api-test",
                ["Jwt:Audience"] = "kdg-boilerplate-client-test"
            });
        
        // Try to load config files if they exist (Docker Compose scenario)
        // These are now optional - Testcontainers doesn't need them
        var localConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.IntegrationLocal.json");
        var azureConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.IntegrationAzure.json");
        
        if (File.Exists(localConfigPath)) {
            builder.AddJsonFile("appsettings.IntegrationLocal.json", optional: true);
        } else if (File.Exists(azureConfigPath)) {
            builder.AddJsonFile("appsettings.IntegrationAzure.json", optional: true);
        }
        
        return builder.Build();
    }

    private IServiceProvider SetupServices()
    {
        var services = new ServiceCollection();

        // Add configuration
        services.AddSingleton(Configuration);

        // Use connection string from test database fixture
        var connectionString = DatabaseFixture.ConnectionString;
        
        // Add database services
        services.AddScoped<IDatabase<NpgsqlConnection, NpgsqlTransaction>>(provider =>
            new KDG.Database.PostgreSQL(connectionString));

        // Add boilerplate services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService>(provider => 
            new AuthService(
                Configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured"),
                Configuration["Jwt:Issuer"] ?? throw new Exception("JWT Issuer not configured"),
                Configuration["Jwt:Audience"] ?? throw new Exception("JWT Audience not configured")
            ));

        // Add controllers
        services.AddScoped<AuthController>();

        // Add HTTP context accessor for controller context
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Add logger factory
        services.AddLogging(builder => builder.AddConsole());

        return services.BuildServiceProvider();
    }

    protected TService GetService<TService>() where TService : notnull
    {
        return ServiceProvider.GetRequiredService<TService>();
    }

    protected AuthController CreateAuthController()
    {
        return GetService<AuthController>();
    }

    protected void SetupControllerContext(ControllerBase controller, Guid userId, string email = "test@example.com")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal,
                RequestServices = ServiceProvider
            }
        };
    }

    protected string GenerateJwtToken(Guid userId, string email = "test@example.com")
    {
        var jwtSettings = Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected async Task<NpgsqlConnection> GetDatabaseConnection()
    {
        var connection = new NpgsqlConnection(DatabaseFixture.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    protected async Task<NpgsqlTransaction> BeginTransaction()
    {
        var connection = await GetDatabaseConnection();
        return await connection.BeginTransactionAsync();
    }
}

