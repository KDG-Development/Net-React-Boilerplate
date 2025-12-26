namespace KDG.Boilerplate.Server.Models.Catalog;

public class ProductDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<ProductImage> Images { get; set; } = [];
    public List<CategoryBreadcrumb> Breadcrumbs { get; set; } = [];

    public ProductDetailResponse() { }

    public ProductDetailResponse(Product product, List<CategoryBreadcrumb> breadcrumbs)
    {
        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        Price = product.Price;
        Images = product.Images;
        Breadcrumbs = breadcrumbs;
    }
}

