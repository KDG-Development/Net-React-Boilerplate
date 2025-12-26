using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Boilerplate.Server.Models.Common;

namespace KDG.Boilerplate.Services;

public interface IProductService
{
    Task<PaginatedResponse<Product>> GetProductsByCategoryAsync(
        Guid categoryId, 
        PaginationParams pagination,
        ProductFilterParams? filters = null);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PaginatedResponse<Product>> GetProductsByCategoryAsync(
        Guid categoryId, 
        PaginationParams pagination,
        ProductFilterParams? filters = null)
    {
        var (products, totalCount) = await _productRepository.GetPaginatedByCategoryAsync(
            categoryId, 
            pagination.Offset, 
            pagination.PageSize,
            filters
        );

        return new PaginatedResponse<Product>(products, pagination.Page, pagination.PageSize, totalCount);
    }
}

