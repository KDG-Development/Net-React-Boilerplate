using KDG.Boilerplate.Server.Models.Crm;
using KDG.Database.Interfaces;
using Npgsql;

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
    private readonly IDatabase<NpgsqlConnection, NpgsqlTransaction> _database;
    private readonly IHeroSlidesRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<HeroSlidesService> _logger;

    public HeroSlidesService(
        IDatabase<NpgsqlConnection, NpgsqlTransaction> database,
        IHeroSlidesRepository repository,
        IBlobStorageService blobStorageService,
        ILogger<HeroSlidesService> logger)
    {
        _database = database;
        _repository = repository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<List<HeroSlide>> GetSlidesAsync(HeroSlideFilters? filters = null)
    {
        return await _database.WithConnection(async conn =>
            await _repository.GetAllAsync(conn, filters));
    }

    public async Task<HeroSlide?> GetSlideByIdAsync(Guid id)
    {
        return await _database.WithConnection(async conn =>
            await _repository.GetByIdAsync(conn, id));
    }

    public async Task<HeroSlide> CreateSlideAsync(Stream imageStream, string fileName, string contentType, CreateHeroSlideDto dto)
    {
        var imageUrl = await _blobStorageService.UploadAsync(imageStream, fileName, contentType);

        return await _database.WithTransaction(async t =>
        {
            var sortOrder = dto.SortOrder > 0 ? dto.SortOrder : await _repository.GetNextSortOrderAsync(t.Connection!);

            var slide = new HeroSlide
            {
                Id = Guid.NewGuid(),
                ImageUrl = imageUrl,
                ButtonText = dto.ButtonText,
                ButtonUrl = dto.ButtonUrl,
                SortOrder = sortOrder,
                IsActive = dto.IsActive
            };

            var created = await _repository.CreateAsync(t, slide);
            _logger.LogInformation("Created hero slide {SlideId}", created.Id);

            return created;
        });
    }

    public async Task<HeroSlide?> UpdateSlideAsync(Guid id, UpdateHeroSlideDto dto)
    {
        return await _database.WithTransaction(async t =>
        {
            var updated = await _repository.UpdateAsync(t, id, dto);
            if (updated != null)
            {
                _logger.LogInformation("Updated hero slide {SlideId}", id);
            }
            return updated;
        });
    }

    public async Task<HeroSlide?> UpdateSlideImageAsync(Guid id, Stream imageStream, string fileName, string contentType)
    {
        var existing = await _database.WithConnection(async conn =>
            await _repository.GetByIdAsync(conn, id));

        if (existing == null) return null;

        await _blobStorageService.DeleteAsync(existing.ImageUrl);

        var newImageUrl = await _blobStorageService.UploadAsync(imageStream, fileName, contentType);

        return await _database.WithTransaction(async t =>
        {
            var updated = await _repository.UpdateImageUrlAsync(t, id, newImageUrl);
            _logger.LogInformation("Updated image for hero slide {SlideId}", id);
            return updated;
        });
    }

    public async Task<bool> DeleteSlideAsync(Guid id)
    {
        var slide = await _database.WithConnection(async conn =>
            await _repository.GetByIdAsync(conn, id));

        if (slide == null) return false;

        await _blobStorageService.DeleteAsync(slide.ImageUrl);

        var deleted = await _database.WithTransaction(async t =>
            await _repository.DeleteAsync(t, id));

        if (deleted)
        {
            _logger.LogInformation("Deleted hero slide {SlideId}", id);
        }
        return deleted;
    }

    public async Task ReorderSlidesAsync(List<Guid> slideIds)
    {
        await _database.WithTransaction(async t =>
        {
            await _repository.ReorderAsync(t, slideIds);
            _logger.LogInformation("Reordered {Count} hero slides", slideIds.Count);
            return true;
        });
    }
}
