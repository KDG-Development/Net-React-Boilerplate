using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Xunit;
using KDG.Boilerplate.Services;
using KDG.Database.Interfaces;
using Npgsql;

namespace KDG.Boilerplate.Server.Tests
{
  public class ProgramTests
  {
    [Fact]
    public void ConfigureServices_WithValidConfiguration_RegistersRequiredServices()
    {
      // Arrange
      var builder = WebApplication.CreateBuilder();
      var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["ConnectionString"] = "test_connection_string",
          ["Jwt:Key"] = "test_key",
          ["Jwt:Issuer"] = "test_issuer",
          ["Jwt:Audience"] = "test_audience",
          ["BaseUrl"] = "http://localhost:5173"
        })
        .Build();
      builder.Configuration.AddConfiguration(configuration);

      // Act
      var connectionString = builder.Configuration["ConnectionString"];
      builder.Services.AddScoped<IDatabase<NpgsqlConnection, NpgsqlTransaction>>(provider => 
        new KDG.Database.PostgreSQL(connectionString!));
      builder.Services.AddScoped<IUserRepository, UserRepository>();
      builder.Services.AddScoped<IAuthService>(provider => new AuthService(
        builder.Configuration["Jwt:Key"]!,
        builder.Configuration["Jwt:Issuer"]!,
        builder.Configuration["Jwt:Audience"]!
      ));

      // Assert
      var serviceProvider = builder.Services.BuildServiceProvider();
      var database = serviceProvider.GetService<IDatabase<NpgsqlConnection, NpgsqlTransaction>>();
      var userRepository = serviceProvider.GetService<IUserRepository>();
      var authService = serviceProvider.GetService<IAuthService>();

      Assert.NotNull(database);
      Assert.NotNull(userRepository);
      Assert.NotNull(authService);
    }
  }
} 