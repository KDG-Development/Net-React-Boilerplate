using KDG.Boilerplate.Services;
using KDG.Boilerplate.Services.Crm;
using KDG.Database.Interfaces;

namespace KDG.Boilerplate.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionString"] 
            ?? throw new Exception("Connection string not configured");

        services.AddScoped<IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction>>(
            _ => new KDG.Database.PostgreSQL(connectionString));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IFavoritesRepository, FavoritesRepository>();
        services.AddScoped<IHeroSlidesRepository, HeroSlidesRepository>();

        return services;
    }
}

