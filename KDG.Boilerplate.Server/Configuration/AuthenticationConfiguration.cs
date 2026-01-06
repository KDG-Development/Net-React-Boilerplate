using System.Text;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace KDG.Boilerplate.Configuration;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new Exception("JWT Issuer not configured");
        var jwtAudience = configuration["Jwt:Audience"] ?? throw new Exception("JWT Audience not configured");

        services.AddScoped<IAuthService>(_ => new AuthService(jwtKey, jwtIssuer, jwtAudience));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        return services;
    }
}

