using KDG.Boilerplate.Services;
using KDG.Boilerplate.Services.Crm;

namespace KDG.Boilerplate.Configuration;

public static class ServicesConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IFavoritesService, FavoritesService>();
        services.AddScoped<IHeroSlidesService, HeroSlidesService>();

        return services;
    }
}
