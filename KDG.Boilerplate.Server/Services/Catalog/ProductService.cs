using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Boilerplate.Server.Models.Common;

namespace KDG.Boilerplate.Services;

public interface IProductService
{
    Task<PaginatedResponse<CatalogProductSummary>> GetCatalogProductsAsync(
        Guid userId,
        PaginationParams pagination,
        Guid? categoryId = null,
        ProductFilterParams? filters = null);
    Task<CatalogProductDetail?> GetCatalogProductByIdAsync(Guid id, Guid userId);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PaginatedResponse<CatalogProductSummary>> GetCatalogProductsAsync(
        Guid userId,
        PaginationParams pagination,
        Guid? categoryId = null,
        ProductFilterParams? filters = null)
    {
        var (products, totalCount) = await _productRepository.GetCatalogProductsAsync(
            pagination.Offset, 
            pagination.PageSize,
            userId,
            categoryId,
            filters
        );

        return new PaginatedResponse<CatalogProductSummary>(products, pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<CatalogProductDetail?> GetCatalogProductByIdAsync(Guid id, Guid userId)
    {
        var result = await _productRepository.GetCatalogProductByIdAsync(id, userId);
        if (result == null)
            return null;

        var (product, isFavorite) = result.Value;

        var breadcrumbs = new List<CategoryBreadcrumb>();
        if (product.CategoryId.HasValue)
        {
            var ancestors = await _categoryRepository.GetAncestorsAsync(product.CategoryId.Value);
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
    }
}
