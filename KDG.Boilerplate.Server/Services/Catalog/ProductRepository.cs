using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Boilerplate.Server.Models.Common;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface IProductRepository
{
    Task<(List<Product> Items, int TotalCount)> GetPaginatedByCategoryAsync(
        Guid categoryId, 
        int offset, 
        int limit,
        ProductFilterParams? filters = null);
}

public class ProductRepository : IProductRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;

    public ProductRepository(IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database)
    {
        _database = database;
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPaginatedByCategoryAsync(
        Guid categoryId, 
        int offset, 
        int limit,
        ProductFilterParams? filters = null)
    {
        return await _database.WithConnection(async connection =>
        {
            var priceFilter = "";
            if (filters?.MinPrice.HasValue == true)
                priceFilter += " AND p.price >= @MinPrice";
            if (filters?.MaxPrice.HasValue == true)
                priceFilter += " AND p.price < @MaxPrice";

            var sql = $@"
                WITH RECURSIVE category_tree AS (
                    SELECT id FROM categories WHERE id = @CategoryId
                    UNION ALL
                    SELECT c.id FROM categories c
                    JOIN category_tree ct ON c.parent_id = ct.id
                )
                SELECT 
                    p.id, 
                    p.category_id AS CategoryId, 
                    p.name, 
                    p.description, 
                    p.price,
                    COUNT(*) OVER() AS TotalCount
                FROM products p
                WHERE p.category_id IN (SELECT id FROM category_tree){priceFilter}
                ORDER BY p.name
                LIMIT @Limit OFFSET @Offset";

            var results = (await connection.QueryAsync<ProductWithCount>(sql, new { 
                CategoryId = categoryId, 
                Limit = limit, 
                Offset = offset,
                MinPrice = filters?.MinPrice,
                MaxPrice = filters?.MaxPrice
            })).ToList();
            
            var totalCount = results.FirstOrDefault()?.TotalCount ?? 0;
            var products = results.Select(r => new Product
            {
                Id = r.Id,
                CategoryId = r.CategoryId,
                Name = r.Name,
                Description = r.Description,
                Price = r.Price
            }).ToList();

            return (products, totalCount);
        });
    }

    private class ProductWithCount
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int TotalCount { get; set; }
    }
}

