using KDG.Database.Interfaces;
using Npgsql;

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
    private readonly IDatabase<NpgsqlConnection, NpgsqlTransaction> _database;
    private readonly IFavoritesRepository _favoritesRepository;

    public FavoritesService(
        IDatabase<NpgsqlConnection, NpgsqlTransaction> database,
        IFavoritesRepository favoritesRepository)
    {
        _database = database;
        _favoritesRepository = favoritesRepository;
    }

    public async Task<FavoritesOperationResult> AddFavoriteAsync(Guid userId, Guid productId)
    {
        var hasOrg = await _database.WithConnection(async conn =>
            await _favoritesRepository.UserHasOrganizationAsync(conn, userId));

        if (!hasOrg)
            return FavoritesOperationResult.Fail("User does not belong to an organization");

        await _database.WithTransaction(async t =>
        {
            await _favoritesRepository.AddFavoriteAsync(t, userId, productId);
            return true;
        });

        return FavoritesOperationResult.Ok();
    }

    public async Task<FavoritesOperationResult> RemoveFavoriteAsync(Guid userId, Guid productId)
    {
        var hasOrg = await _database.WithConnection(async conn =>
            await _favoritesRepository.UserHasOrganizationAsync(conn, userId));

        if (!hasOrg)
            return FavoritesOperationResult.Fail("User does not belong to an organization");

        await _database.WithTransaction(async t =>
        {
            await _favoritesRepository.RemoveFavoriteAsync(t, userId, productId);
            return true;
        });

        return FavoritesOperationResult.Ok();
    }
}
