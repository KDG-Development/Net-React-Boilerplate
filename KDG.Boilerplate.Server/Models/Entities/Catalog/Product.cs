namespace KDG.Boilerplate.Server.Models.Entities.Catalog;

/// <summary>
/// Full product entity with category and multiple images.
/// </summary>
public class Product : ProductCore
{
    public Guid? CategoryId { get; set; }
    public List<ProductImage> Images { get; set; } = [];
}

