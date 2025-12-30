namespace KDG.Boilerplate.Services;

public class FavoritesOperationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static FavoritesOperationResult Ok() => new() { Success = true };
    public static FavoritesOperationResult Fail(string error) => new() { Success = false, Error = error };
}

public interface IFavoritesService
{
    Task<FavoritesOperationResult> AddFavoriteAsync(Guid userId, Guid productId);
    Task<FavoritesOperationResult> RemoveFavoriteAsync(Guid userId, Guid productId);
}

public class FavoritesService : IFavoritesService
{
    private readonly IFavoritesRepository _favoritesRepository;

    public FavoritesService(IFavoritesRepository favoritesRepository)
    {
        _favoritesRepository = favoritesRepository;
    }

    public async Task<FavoritesOperationResult> AddFavoriteAsync(Guid userId, Guid productId)
    {
        var hasOrg = await _favoritesRepository.UserHasOrganizationAsync(userId);
        if (!hasOrg)
            return FavoritesOperationResult.Fail("User does not belong to an organization");

        await _favoritesRepository.AddFavoriteAsync(userId, productId);
        return FavoritesOperationResult.Ok();
    }

    public async Task<FavoritesOperationResult> RemoveFavoriteAsync(Guid userId, Guid productId)
    {
        var hasOrg = await _favoritesRepository.UserHasOrganizationAsync(userId);
        if (!hasOrg)
            return FavoritesOperationResult.Fail("User does not belong to an organization");

        await _favoritesRepository.RemoveFavoriteAsync(userId, productId);
        return FavoritesOperationResult.Ok();
    }
}

