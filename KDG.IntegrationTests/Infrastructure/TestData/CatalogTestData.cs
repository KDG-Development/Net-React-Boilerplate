using Dapper;
using Npgsql;

namespace KDG.IntegrationTests.Infrastructure.TestData;

/// <summary>
/// Test data helpers for products and categories.
/// 
/// NOTE: Direct SQL is temporary. Catalog data will be synced from NetSuite.
/// Update to use the service layer once implemented.
/// </summary>
public class CatalogTestData
{
    private readonly Func<Task<NpgsqlConnection>> _getConnection;

    public CatalogTestData(Func<Task<NpgsqlConnection>> getConnection)
    {
        _getConnection = getConnection;
    }

    public async Task<Guid> CreateProduct(string name, decimal price = 99.99m)
    {
        using var connection = await _getConnection();
        
        var productId = Guid.NewGuid();
        await connection.ExecuteAsync(@"
            INSERT INTO products (id, name, description, price)
            VALUES (@Id, @Name, 'Test product', @Price)",
            new { Id = productId, Name = name, Price = price });

        return productId;
    }

    public async Task<Guid> CreateCategory(string name, Guid? parentId = null)
    {
        using var connection = await _getConnection();
        
        var id = Guid.NewGuid();
        await connection.ExecuteAsync(@"
            INSERT INTO categories (id, parent_id, name)
            VALUES (@Id, @ParentId, @Name)",
            new { Id = id, ParentId = parentId, Name = name });

        return id;
    }

    public async Task CleanupTestCategories()
    {
        using var connection = await _getConnection();
        await connection.ExecuteAsync(@"
            DELETE FROM categories 
            WHERE name LIKE 'Test Category%' 
               OR name LIKE 'Test Root%'
               OR name LIKE 'Test Sub%'
               OR name LIKE 'Test Leaf%'");
    }

    public async Task CleanupTestProducts()
    {
        using var connection = await _getConnection();
        await connection.ExecuteAsync("DELETE FROM products WHERE name LIKE 'Test%'");
    }
}

