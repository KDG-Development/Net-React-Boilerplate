using KDG.Boilerplate.Server.Models.Entities.Catalog;
using KDG.Boilerplate.Server.Models.Common;
using KDG.Boilerplate.Server.Models.Requests.Common;
using KDG.Boilerplate.Server.Models.Requests.Products;
using KDG.Database.Interfaces;
using Npgsql;

namespace KDG.Boilerplate.Services;

public interface IProductService
{
    Task<PaginatedResponse<CatalogProductSummary>> GetCatalogProductsAsync(
        Guid userId,
        PaginationParams pagination,
        Guid? categoryId = null,
        ProductFilters? filters = null);
    Task<CatalogProductDetail?> GetCatalogProductByIdAsync(Guid id, Guid userId);
}

public class ProductService : IProductService
{
    private readonly IDatabase<NpgsqlConnection, NpgsqlTransaction> _database;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(
        IDatabase<NpgsqlConnection, NpgsqlTransaction> database,
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository)
    {
        _database = database;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PaginatedResponse<CatalogProductSummary>> GetCatalogProductsAsync(
        Guid userId,
        PaginationParams pagination,
        Guid? categoryId = null,
        ProductFilters? filters = null)
    {
        var (products, totalCount) = await _database.WithConnection(async conn =>
            await _productRepository.GetCatalogProductsAsync(
                conn,
                pagination.Offset, 
                pagination.PageSize,
                userId,
                categoryId,
                filters
            ));

        return new PaginatedResponse<CatalogProductSummary>(products, pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<CatalogProductDetail?> GetCatalogProductByIdAsync(Guid id, Guid userId)
    {
        return await _database.WithConnection(async conn =>
        {
            var result = await _productRepository.GetCatalogProductByIdAsync(conn, id, userId);
            if (result == null)
                return null;

            var (product, isFavorite) = result.Value;

            var breadcrumbs = new List<CategoryBreadcrumb>();
            if (product.CategoryId.HasValue)
            {
                var ancestors = await _categoryRepository.GetAncestorsAsync(conn, product.CategoryId.Value);
                breadcrumbs = ancestors.Select(c => new CategoryBreadcrumb
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.FullPath
                }).ToList();
            }

            return new CatalogProductDetail
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Images = product.Images,
                IsFavorite = isFavorite,
                Breadcrumbs = breadcrumbs
            };
        });
    }
}
