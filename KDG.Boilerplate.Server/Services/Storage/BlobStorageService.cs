using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace KDG.Boilerplate.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType);
    Task DeleteAsync(string blobUrl);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string? _publicBaseUrl;
    private bool _containerInitialized;

    public BlobStorageService(string connectionString, string containerName, ILogger<BlobStorageService> logger, string? publicBaseUrl = null)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _logger = logger;
        _publicBaseUrl = publicBaseUrl;
        _containerInitialized = false;
    }

    private async Task EnsureContainerExistsAsync()
    {
        if (_containerInitialized) return;
        
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        _containerInitialized = true;
        _logger.LogInformation("Blob container initialized");
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        await EnsureContainerExistsAsync();
        
        var blobName = $"{Guid.NewGuid()}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
        
        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
        
        _logger.LogInformation("Uploaded blob {BlobName} to container", blobName);

        if (!string.IsNullOrEmpty(_publicBaseUrl))
        {
            return $"{_publicBaseUrl}/{_containerClient.Name}/{blobName}";
        }
        
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl)
    {
        if (string.IsNullOrEmpty(blobUrl)) return;

        var uri = new Uri(blobUrl);
        var blobName = string.Join("/", uri.Segments.Skip(2));
        
        var blobClient = _containerClient.GetBlobClient(blobName);
        var deleted = await blobClient.DeleteIfExistsAsync();
        
        if (deleted)
        {
            _logger.LogInformation("Deleted blob {BlobName} from container", blobName);
        }
    }
}

