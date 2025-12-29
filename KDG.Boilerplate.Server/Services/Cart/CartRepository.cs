using KDG.Boilerplate.Server.Models.Cart;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface ICartRepository
{
    Task<List<CartProduct>> GetCartAsync(Guid userId);
    Task ReplaceCartAsync(Guid userId, List<UserCartItem> items);
}

public class CartRepository : ICartRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;
    private readonly IProductRepository _productRepository;

    public CartRepository(
        IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database,
        IProductRepository productRepository)
    {
        _database = database;
        _productRepository = productRepository;
    }

    public async Task<List<CartProduct>> GetCartAsync(Guid userId)
    {
        var cartItems = await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT product_id AS ProductId, quantity AS Quantity
                FROM user_cart_items
                WHERE user_id = @UserId";

            return (await connection.QueryAsync<UserCartItem>(sql, new { UserId = userId })).ToList();
        });

        if (cartItems.Count == 0)
            return [];

        var productIds = cartItems.Select(i => i.ProductId).ToArray();
        var products = await _productRepository.GetMetaByIdsAsync(productIds);
        var productsById = products.ToDictionary(p => p.Id);

        return cartItems
            .Where(i => productsById.ContainsKey(i.ProductId))
            .Select(i => new CartProduct
            {
                Product = productsById[i.ProductId],
                Quantity = i.Quantity
            })
            .ToList();
    }

    public async Task ReplaceCartAsync(Guid userId, List<UserCartItem> items)
    {
        await _database.WithConnection(async connection =>
        {
            using var transaction = await connection.BeginTransactionAsync();
            
            await connection.ExecuteAsync(
                "DELETE FROM user_cart_items WHERE user_id = @UserId",
                new { UserId = userId },
                transaction
            );

            if (items.Count > 0)
            {
                var sql = @"
                    INSERT INTO user_cart_items (user_id, product_id, quantity)
                    VALUES (@UserId, @ProductId, @Quantity)";

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }, transaction);
                }
            }

            await transaction.CommitAsync();
            return 0;
        });
    }
}
