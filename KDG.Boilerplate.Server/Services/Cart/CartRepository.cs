using KDG.Boilerplate.Server.Models.Cart;
using Npgsql;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface ICartRepository
{
    Task<List<UserCartItem>> GetCartItemsAsync(NpgsqlConnection conn, Guid userId);
    Task ReplaceCartAsync(NpgsqlTransaction t, Guid userId, List<UserCartItem> items);
}

public class CartRepository : ICartRepository
{
    public async Task<List<UserCartItem>> GetCartItemsAsync(NpgsqlConnection conn, Guid userId)
    {
        var sql = @"
            SELECT product_id AS ProductId, quantity AS Quantity
            FROM user_cart_items
            WHERE user_id = @UserId";

        return (await conn.QueryAsync<UserCartItem>(sql, new { UserId = userId })).ToList();
    }

    public async Task ReplaceCartAsync(NpgsqlTransaction t, Guid userId, List<UserCartItem> items)
    {
        await t.Connection!.ExecuteAsync(
            "DELETE FROM user_cart_items WHERE user_id = @UserId",
            new { UserId = userId },
            t
        );

        if (items.Count > 0)
        {
            var sql = @"
                INSERT INTO user_cart_items (user_id, product_id, quantity)
                VALUES (@UserId, @ProductId, @Quantity)";

            foreach (var item in items)
            {
                await t.Connection!.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }, t);
            }
        }
    }
}
