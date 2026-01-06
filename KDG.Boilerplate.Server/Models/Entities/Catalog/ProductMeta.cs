namespace KDG.Boilerplate.Server.Models.Entities.Catalog;

/// <summary>
/// Lightweight product reference with a single image (e.g., for cart items).
/// </summary>
public class ProductMeta : ProductCore
{
    public ProductImage? Image { get; set; }
}

