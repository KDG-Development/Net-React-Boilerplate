using Dapper;
using KDG.Boilerplate.Server.Models.Entities.Cart;
using KDG.Boilerplate.Services;
using KDG.IntegrationTests.Infrastructure;

namespace KDG.IntegrationTests.Tests.Cart;

/// <summary>
/// Integration tests for cart persistence that verify cart operations
/// work correctly with a real database.
/// 
/// Business context: The shopping cart is the critical path to revenue.
/// Lost cart items mean abandoned purchases. Corrupted carts mean customer
/// frustration.
/// </summary>
public class CartPersistenceTests : IntegrationTestBase
{
    public CartPersistenceTests(DatabaseTestFixture fixture) : base(fixture) { }

    #region Test Data Setup

    private async Task CleanupTestData(Guid userId)
    {
        using var connection = await GetDatabaseConnection();
        await connection.ExecuteAsync("DELETE FROM user_cart_items WHERE user_id = @UserId", new { UserId = userId });
    }

    #endregion

    /// <summary>
    /// Verifies that replacing a cart atomically removes old items and adds new ones.
    /// 
    /// Business context: Cart sync from frontend must be idempotent. When customer
    /// updates cart, the entire state is replaced. Partial failures would leave
    /// carts inconsistent.
    /// 
    /// Real-world scenario: Customer has 3 items, removes one, changes quantity
    /// of another. Frontend sends new state. Old items removed, new items inserted.
    /// </summary>
    [Fact]
    public async Task ReplaceCart_AtomicallyReplacesAllItems()
    {
        var userId = await TestData.Users.Create();
        var product1 = await TestData.Catalog.CreateProduct("Widget A", 29.99m);
        var product2 = await TestData.Catalog.CreateProduct("Widget B", 49.99m);
        var product3 = await TestData.Catalog.CreateProduct("Widget C", 19.99m);

        try
        {
            var cartService = GetService<ICartService>();

            // Initial cart
            await cartService.ReplaceCartAsync(userId, new List<UserCartItem>
            {
                new() { UserId = userId, ProductId = product1, Quantity = 2 },
                new() { UserId = userId, ProductId = product2, Quantity = 1 }
            });

            // Replace with different items
            await cartService.ReplaceCartAsync(userId, new List<UserCartItem>
            {
                new() { UserId = userId, ProductId = product2, Quantity = 3 },
                new() { UserId = userId, ProductId = product3, Quantity = 5 }
            });

            var cart = await cartService.GetCartAsync(userId);

            Assert.Equal(2, cart.Count);
            Assert.DoesNotContain(cart, i => i.Product.Id == product1);
            Assert.Contains(cart, i => i.Product.Id == product2 && i.Quantity == 3);
            Assert.Contains(cart, i => i.Product.Id == product3 && i.Quantity == 5);

            // Clear cart (empty list)
            await cartService.ReplaceCartAsync(userId, new List<UserCartItem>());
            Assert.Empty(await cartService.GetCartAsync(userId));
        }
        finally
        {
            await CleanupTestData(userId);
        }
    }

    /// <summary>
    /// Verifies that cart replace is idempotent - same request yields same result.
    /// 
    /// Business context: Network issues may cause request retries. If customer
    /// clicks "Update Cart" and request is sent twice, cart should end up
    /// correct without duplicate items.
    /// 
    /// Real-world scenario: Customer on slow connection updates cart. Request
    /// times out, app retries. Cart ends up correct, not doubled.
    /// </summary>
    [Fact]
    public async Task ReplaceCart_IsIdempotent()
    {
        var userId = await TestData.Users.Create();
        var product = await TestData.Catalog.CreateProduct("Idempotent Widget", 15.99m);

        try
        {
            var cartService = GetService<ICartService>();

            var cartItems = new List<UserCartItem>
            {
                new() { UserId = userId, ProductId = product, Quantity = 7 }
            };

            await cartService.ReplaceCartAsync(userId, cartItems);
            await cartService.ReplaceCartAsync(userId, cartItems);
            await cartService.ReplaceCartAsync(userId, cartItems);

            var cart = await cartService.GetCartAsync(userId);
            Assert.Single(cart);
            Assert.Equal(7, cart[0].Quantity);
        }
        finally
        {
            await CleanupTestData(userId);
        }
    }

    /// <summary>
    /// Verifies that GetCart returns cart items with product metadata.
    /// 
    /// Business context: Cart display needs product names, prices, and images.
    /// The service joins cart items with product data for frontend rendering.
    /// 
    /// Real-world scenario: Customer opens cart page and sees product names
    /// and prices, not just IDs.
    /// </summary>
    [Fact]
    public async Task GetCart_ReturnsProductMetadata()
    {
        var userId = await TestData.Users.Create();
        var product = await TestData.Catalog.CreateProduct("Detailed Widget", 99.99m);

        try
        {
            var cartService = GetService<ICartService>();

            await cartService.ReplaceCartAsync(userId, new List<UserCartItem>
            {
                new() { UserId = userId, ProductId = product, Quantity = 2 }
            });

            var cart = await cartService.GetCartAsync(userId);

            Assert.Single(cart);
            Assert.Equal(product, cart[0].Product.Id);
            Assert.Equal("Detailed Widget", cart[0].Product.Name);
            Assert.Equal(2, cart[0].Quantity);
        }
        finally
        {
            await CleanupTestData(userId);
        }
    }

}
