using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Boilerplate.Server.Models.Common;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface IProductRepository
{
    Task<(List<CatalogProductSummary> Items, int TotalCount)> GetCatalogProductsAsync(
        int offset, 
        int limit,
        Guid userId,
        Guid? categoryId = null,
        ProductFilterParams? filters = null);
    Task<(Product Product, bool IsFavorite)?> GetCatalogProductByIdAsync(Guid id, Guid userId);
    Task<List<ProductMeta>> GetMetaByIdsAsync(Guid[] ids);
}

public class ProductRepository : IProductRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;

    public ProductRepository(IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database)
    {
        _database = database;
    }

    public async Task<(List<CatalogProductSummary> Items, int TotalCount)> GetCatalogProductsAsync(
        int offset, 
        int limit,
        Guid userId,
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
            if (filters?.FavoritesOnly == true)
                conditions.Add("of.product_id IS NOT NULL");

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
                        (of.product_id IS NOT NULL) AS IsFavorite,
                        COUNT(*) OVER() AS TotalCount
                    FROM products p
                    LEFT JOIN users u ON u.id = @UserId
                    LEFT JOIN organization_favorites of ON of.product_id = p.id AND of.organization_id = u.organization_id
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
                        (of.product_id IS NOT NULL) AS IsFavorite,
                        COUNT(*) OVER() AS TotalCount
                    FROM products p
                    LEFT JOIN users u ON u.id = @UserId
                    LEFT JOIN organization_favorites of ON of.product_id = p.id AND of.organization_id = u.organization_id
                    {whereClause}
                    ORDER BY p.name
                    LIMIT @Limit OFFSET @Offset";
            }

            var results = (await connection.QueryAsync<ProductListItemRecord>(sql, new { 
                CategoryId = categoryId, 
                UserId = userId,
                Limit = limit, 
                Offset = offset,
                MinPrice = filters?.MinPrice,
                MaxPrice = filters?.MaxPrice,
                SearchTerm = searchQuery
            })).ToList();
            
            var totalCount = results.FirstOrDefault()?.TotalCount ?? 0;
            var products = results.Select(r => new CatalogProductSummary
            {
                Id = r.Id,
                CategoryId = r.CategoryId,
                Name = r.Name,
                Description = r.Description,
                Price = r.Price,
                IsFavorite = r.IsFavorite
            }).ToList();

            // Fetch images for all products in a single query
            if (products.Count > 0)
            {
                var productIds = products.Select(p => p.Id).ToArray();
                var images = await connection.QueryAsync<ProductImage>(
                    @"SELECT id, product_id AS ProductId, src, sort_order AS SortOrder
                      FROM product_images
                      WHERE product_id = ANY(@ProductIds)
                      ORDER BY sort_order",
                    new { ProductIds = productIds }
                );

                var imagesByProduct = images.ToLookup(i => i.ProductId);

                foreach (var product in products)
                {
                    product.Images = imagesByProduct[product.Id].ToList();
                }
            }

            return (products, totalCount);
        });
    }

    public async Task<(Product Product, bool IsFavorite)?> GetCatalogProductByIdAsync(Guid id, Guid userId)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT 
                    p.id, 
                    p.category_id AS CategoryId, 
                    p.name, 
                    p.description, 
                    p.price,
                    (of.product_id IS NOT NULL) AS IsFavorite
                FROM products p
                LEFT JOIN users u ON u.id = @UserId
                LEFT JOIN organization_favorites of ON of.product_id = p.id AND of.organization_id = u.organization_id
                WHERE p.id = @Id";

            var result = await connection.QueryFirstOrDefaultAsync<ProductWithFavoriteRecord>(sql, new { Id = id, UserId = userId });
            
            if (result == null)
                return ((Product, bool)?)null;

            var product = new Product
            {
                Id = result.Id,
                CategoryId = result.CategoryId,
                Name = result.Name,
                Description = result.Description,
                Price = result.Price
            };

            // Fetch images
            var images = await connection.QueryAsync<ProductImage>(
                @"SELECT id, product_id AS ProductId, src, sort_order AS SortOrder
                  FROM product_images
                  WHERE product_id = @ProductId
                  ORDER BY sort_order",
                new { ProductId = id }
            );
            product.Images = images.ToList();

            return (product, result.IsFavorite);
        });
    }

    public async Task<List<ProductMeta>> GetMetaByIdsAsync(Guid[] ids)
    {
        if (ids.Length == 0)
            return [];

        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT 
                    p.id, 
                    p.name, 
                    p.description, 
                    p.price
                FROM products p
                WHERE p.id = ANY(@Ids)";

            var products = (await connection.QueryAsync<ProductMeta>(sql, new { Ids = ids })).ToList();

            if (products.Count > 0)
            {
                var productIds = products.Select(p => p.Id).ToArray();
                var images = await connection.QueryAsync<ProductImage>(
                    @"SELECT DISTINCT ON (product_id) 
                        id, product_id AS ProductId, src, sort_order AS SortOrder
                      FROM product_images
                      WHERE product_id = ANY(@ProductIds)
                      ORDER BY product_id, sort_order",
                    new { ProductIds = productIds }
                );

                var imageByProduct = images.ToDictionary(i => i.ProductId);
                foreach (var product in products)
                {
                    if (imageByProduct.TryGetValue(product.Id, out var image))
                        product.Image = image;
                }
            }

            return products;
        });
    }

    private class ProductListItemRecord
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsFavorite { get; set; }
        public int TotalCount { get; set; }
    }

    private class ProductWithFavoriteRecord
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsFavorite { get; set; }
    }
}
