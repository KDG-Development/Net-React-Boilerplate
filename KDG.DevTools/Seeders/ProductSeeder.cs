using Bogus;
using Dapper;
using KDG.Database;

namespace KDG.DevTools.Seeders;

public class ProductSeeder : ISeeder
{
    private readonly PostgreSQL _database;

    public string Name => "Products";

    public ProductSeeder(PostgreSQL database)
    {
        _database = database;
    }

    public async Task<int> SeedAsync(int count)
    {
        // First, get all existing category IDs
        var categoryIds = await _database.WithConnection(async conn =>
        {
            return (await conn.QueryAsync<Guid>("SELECT id FROM categories")).ToList();
        });

        if (categoryIds.Count == 0)
        {
            Console.WriteLine("  Warning: No categories found. Products will be created without categories.");
        }

        var faker = new Faker();
        var productFaker = new Faker<ProductData>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(5, 500)))
            .RuleFor(p => p.CategoryId, f => categoryIds.Count > 0 ? f.PickRandom(categoryIds) : null);

        var insertedCount = 0;

        for (var i = 0; i < count; i++)
        {
            var product = productFaker.Generate();

            await _database.WithConnection(async conn =>
            {
                await conn.ExecuteAsync(
                    @"INSERT INTO products (id, category_id, name, description, price) 
                      VALUES (@Id, @CategoryId, @Name, @Description, @Price)",
                    new
                    {
                        product.Id,
                        product.CategoryId,
                        product.Name,
                        product.Description,
                        product.Price
                    }
                );
                return true;
            });

            insertedCount++;
            
            if (insertedCount % 10 == 0 || insertedCount == count)
            {
                Console.WriteLine($"  Created {insertedCount}/{count} products...");
            }
        }

        return insertedCount;
    }

    private class ProductData
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
