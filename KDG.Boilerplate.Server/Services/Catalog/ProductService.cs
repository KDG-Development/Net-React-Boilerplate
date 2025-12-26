using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Boilerplate.Server.Models.Common;

namespace KDG.Boilerplate.Services;

public interface IProductService
{
    Task<PaginatedResponse<Product>> GetProductsAsync(
        PaginationParams pagination,
        Guid? categoryId = null,
        ProductFilterParams? filters = null);
    Task<ProductDetailResponse?> GetProductByIdAsync(Guid id);
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

    public async Task<PaginatedResponse<Product>> GetProductsAsync(
        PaginationParams pagination,
        Guid? categoryId = null,
        ProductFilterParams? filters = null)
    {
        var (products, totalCount) = await _productRepository.GetPaginatedAsync(
            pagination.Offset, 
            pagination.PageSize,
            categoryId,
            filters
        );

        return new PaginatedResponse<Product>(products, pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<ProductDetailResponse?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

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

        return new ProductDetailResponse(product, breadcrumbs);
    }
}
