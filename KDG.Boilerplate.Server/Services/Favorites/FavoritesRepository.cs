using Npgsql;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface IFavoritesRepository
{
    Task AddFavoriteAsync(NpgsqlTransaction t, Guid userId, Guid productId);
    Task RemoveFavoriteAsync(NpgsqlTransaction t, Guid userId, Guid productId);
    Task<bool> UserHasOrganizationAsync(NpgsqlConnection conn, Guid userId);
}

public class FavoritesRepository : IFavoritesRepository
{
    public async Task AddFavoriteAsync(NpgsqlTransaction t, Guid userId, Guid productId)
    {
        var sql = @"
            INSERT INTO organization_favorites (organization_id, product_id)
            SELECT u.organization_id, @ProductId
            FROM users u
            WHERE u.id = @UserId AND u.organization_id IS NOT NULL
            ON CONFLICT (organization_id, product_id) DO NOTHING";

        await t.Connection!.ExecuteAsync(sql, new { UserId = userId, ProductId = productId }, t);
    }

    public async Task RemoveFavoriteAsync(NpgsqlTransaction t, Guid userId, Guid productId)
    {
        var sql = @"
            DELETE FROM organization_favorites
            WHERE product_id = @ProductId
            AND organization_id = (SELECT organization_id FROM users WHERE id = @UserId)";

        await t.Connection!.ExecuteAsync(sql, new { UserId = userId, ProductId = productId }, t);
    }

    public async Task<bool> UserHasOrganizationAsync(NpgsqlConnection conn, Guid userId)
    {
        var sql = "SELECT organization_id FROM users WHERE id = @UserId";
        var orgId = await conn.QueryFirstOrDefaultAsync<Guid?>(sql, new { UserId = userId });
        return orgId.HasValue;
    }
}
