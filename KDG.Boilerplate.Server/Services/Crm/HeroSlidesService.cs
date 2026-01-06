using KDG.Boilerplate.Server.Models.Crm;

namespace KDG.Boilerplate.Services.Crm;

public interface IHeroSlidesService
{
    Task<List<HeroSlide>> GetSlidesAsync(HeroSlideFilters? filters = null);
    Task<HeroSlide?> GetSlideByIdAsync(Guid id);
    Task<HeroSlide> CreateSlideAsync(Stream imageStream, string fileName, string contentType, CreateHeroSlideDto dto);
    Task<HeroSlide?> UpdateSlideAsync(Guid id, UpdateHeroSlideDto dto);
    Task<HeroSlide?> UpdateSlideImageAsync(Guid id, Stream imageStream, string fileName, string contentType);
    Task<bool> DeleteSlideAsync(Guid id);
    Task ReorderSlidesAsync(List<Guid> slideIds);
}

public class HeroSlidesService : IHeroSlidesService
{
    private readonly IHeroSlidesRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<HeroSlidesService> _logger;

    public HeroSlidesService(
        IHeroSlidesRepository repository,
        IBlobStorageService blobStorageService,
        ILogger<HeroSlidesService> logger)
    {
        _repository = repository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<List<HeroSlide>> GetSlidesAsync(HeroSlideFilters? filters = null)
    {
        return await _repository.GetAllAsync(filters);
    }

    public async Task<HeroSlide?> GetSlideByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<HeroSlide> CreateSlideAsync(Stream imageStream, string fileName, string contentType, CreateHeroSlideDto dto)
    {
        var imageUrl = await _blobStorageService.UploadAsync(imageStream, fileName, contentType);
        
        var sortOrder = dto.SortOrder > 0 ? dto.SortOrder : await _repository.GetNextSortOrderAsync();
        
        var slide = new HeroSlide
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            ButtonText = dto.ButtonText,
            ButtonUrl = dto.ButtonUrl,
            SortOrder = sortOrder,
            IsActive = dto.IsActive
        };

        var created = await _repository.CreateAsync(slide);
        _logger.LogInformation("Created hero slide {SlideId}", created.Id);
        
        return created;
    }

    public async Task<HeroSlide?> UpdateSlideAsync(Guid id, UpdateHeroSlideDto dto)
    {
        var updated = await _repository.UpdateAsync(id, dto);
        if (updated != null)
        {
            _logger.LogInformation("Updated hero slide {SlideId}", id);
        }
        return updated;
    }

    public async Task<HeroSlide?> UpdateSlideImageAsync(Guid id, Stream imageStream, string fileName, string contentType)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        await _blobStorageService.DeleteAsync(existing.ImageUrl);
        
        var newImageUrl = await _blobStorageService.UploadAsync(imageStream, fileName, contentType);
        
        var updated = await _repository.UpdateImageUrlAsync(id, newImageUrl);
        
        _logger.LogInformation("Updated image for hero slide {SlideId}", id);
        
        return updated;
    }

    public async Task<bool> DeleteSlideAsync(Guid id)
    {
        var slide = await _repository.GetByIdAsync(id);
        if (slide == null) return false;

        await _blobStorageService.DeleteAsync(slide.ImageUrl);
        
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Deleted hero slide {SlideId}", id);
        }
        return deleted;
    }

    public async Task ReorderSlidesAsync(List<Guid> slideIds)
    {
        await _repository.ReorderAsync(slideIds);
        _logger.LogInformation("Reordered {Count} hero slides", slideIds.Count);
    }
}

