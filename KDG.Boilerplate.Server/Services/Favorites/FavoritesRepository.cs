using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface IFavoritesRepository
{
    Task AddFavoriteAsync(Guid userId, Guid productId);
    Task RemoveFavoriteAsync(Guid userId, Guid productId);
    Task<bool> UserHasOrganizationAsync(Guid userId);
}

public class FavoritesRepository : IFavoritesRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;

    public FavoritesRepository(IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database)
    {
        _database = database;
    }

    public async Task AddFavoriteAsync(Guid userId, Guid productId)
    {
        await _database.WithConnection(async connection =>
        {
            var sql = @"
                INSERT INTO organization_favorites (organization_id, product_id)
                SELECT u.organization_id, @ProductId
                FROM users u
                WHERE u.id = @UserId AND u.organization_id IS NOT NULL
                ON CONFLICT (organization_id, product_id) DO NOTHING";

            await connection.ExecuteAsync(sql, new { UserId = userId, ProductId = productId });
            return 0;
        });
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid productId)
    {
        await _database.WithConnection(async connection =>
        {
            var sql = @"
                DELETE FROM organization_favorites
                WHERE product_id = @ProductId
                AND organization_id = (SELECT organization_id FROM users WHERE id = @UserId)";

            await connection.ExecuteAsync(sql, new { UserId = userId, ProductId = productId });
            return 0;
        });
    }

    public async Task<bool> UserHasOrganizationAsync(Guid userId)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = "SELECT organization_id FROM users WHERE id = @UserId";
            var orgId = await connection.QueryFirstOrDefaultAsync<Guid?>(sql, new { UserId = userId });
            return orgId.HasValue;
        });
    }
}

