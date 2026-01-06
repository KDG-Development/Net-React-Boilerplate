using KDG.Boilerplate.Services;

namespace KDG.Boilerplate.Configuration;

public static class BlobStorageConfiguration
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorage:ConnectionString"] 
            ?? throw new Exception("Blob Storage connection string not configured");
        var containerName = configuration["BlobStorage:ContainerName"] ?? "uploads";
        var publicBaseUrl = configuration["BlobStorage:PublicBaseUrl"];

        services.AddScoped<IBlobStorageService>(provider => new BlobStorageService(
            connectionString,
            containerName,
            provider.GetRequiredService<ILogger<BlobStorageService>>(),
            publicBaseUrl
        ));

        return services;
    }
}

