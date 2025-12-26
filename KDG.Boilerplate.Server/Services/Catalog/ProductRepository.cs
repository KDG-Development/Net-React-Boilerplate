using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Boilerplate.Server.Models.Common;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface IProductRepository
{
    Task<(List<Product> Items, int TotalCount)> GetPaginatedAsync(
        int offset, 
        int limit,
        Guid? categoryId = null,
        ProductFilterParams? filters = null);
}

public class ProductRepository : IProductRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;

    public ProductRepository(IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database)
    {
        _database = database;
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPaginatedAsync(
        int offset, 
        int limit,
        Guid? categoryId = null,
        ProductFilterParams? filters = null)
    {
        return await _database.WithConnection(async connection =>
        {
            var conditions = new List<string>();
            
            if (filters?.MinPrice.HasValue == true)
                conditions.Add("p.price >= @MinPrice");
            if (filters?.MaxPrice.HasValue == true)
                conditions.Add("p.price < @MaxPrice");
            var searchQuery = filters?.GetSearchQuery();
            if (searchQuery != null)
                conditions.Add("p.search_vector @@ to_tsquery('english', @SearchTerm)");

            string sql;
            if (categoryId.HasValue)
            {
                var categoryCondition = "p.category_id IN (SELECT id FROM category_tree)";
                conditions.Insert(0, categoryCondition);
                
                sql = $@"
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
                    WHERE {string.Join(" AND ", conditions)}
                    ORDER BY p.name
                    LIMIT @Limit OFFSET @Offset";
            }
            else
            {
                var whereClause = conditions.Count > 0 
                    ? "WHERE " + string.Join(" AND ", conditions)
                    : "";

                sql = $@"
                    SELECT 
                        p.id, 
                        p.category_id AS CategoryId, 
                        p.name, 
                        p.description, 
                        p.price,
                        COUNT(*) OVER() AS TotalCount
                    FROM products p
                    {whereClause}
                    ORDER BY p.name
                    LIMIT @Limit OFFSET @Offset";
            }

            var results = (await connection.QueryAsync<ProductWithCount>(sql, new { 
                CategoryId = categoryId, 
                Limit = limit, 
                Offset = offset,
                MinPrice = filters?.MinPrice,
                MaxPrice = filters?.MaxPrice,
                SearchTerm = searchQuery
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
