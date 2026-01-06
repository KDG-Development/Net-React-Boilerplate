using KDG.Boilerplate.Server.Models.Cart;
using KDG.Database.Interfaces;
using Npgsql;

namespace KDG.Boilerplate.Services;

public interface ICartService
{
    Task<List<CartProduct>> GetCartAsync(Guid userId);
    Task ReplaceCartAsync(Guid userId, List<UserCartItem> items);
}

public class CartService : ICartService
{
    private readonly IDatabase<NpgsqlConnection, NpgsqlTransaction> _database;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(
        IDatabase<NpgsqlConnection, NpgsqlTransaction> database,
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _database = database;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<List<CartProduct>> GetCartAsync(Guid userId)
    {
        return await _database.WithConnection(async conn =>
        {
            var cartItems = await _cartRepository.GetCartItemsAsync(conn, userId);

            if (cartItems.Count == 0)
                return new List<CartProduct>();

            var productIds = cartItems.Select(i => i.ProductId).ToArray();
            var products = await _productRepository.GetMetaByIdsAsync(conn, productIds);
            var productsById = products.ToDictionary(p => p.Id);

            return cartItems
                .Where(i => productsById.ContainsKey(i.ProductId))
                .Select(i => new CartProduct
                {
                    Product = productsById[i.ProductId],
                    Quantity = i.Quantity
                })
                .ToList();
        });
    }

    public async Task ReplaceCartAsync(Guid userId, List<UserCartItem> items)
    {
        await _database.WithTransaction(async t =>
        {
            await _cartRepository.ReplaceCartAsync(t, userId, items);
            return true;
        });
    }
}
