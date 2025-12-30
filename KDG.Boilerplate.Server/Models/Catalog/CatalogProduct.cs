namespace KDG.Boilerplate.Server.Models.Catalog;

/// <summary>
/// Base DTO for products in the catalog context (user-facing endpoints).
/// Inherits core product properties and adds user-specific context like IsFavorite.
/// </summary>
public class CatalogProduct : Product
{
    public bool IsFavorite { get; set; }
}

/// <summary>
/// Product summary for catalog listings.
/// Inherits all properties from Product (including CategoryId) plus IsFavorite.
/// </summary>
public class CatalogProductSummary : CatalogProduct
{
}

/// <summary>
/// Product detail for catalog product pages.
/// </summary>
public class CatalogProductDetail : CatalogProduct
{
    public List<CategoryBreadcrumb> Breadcrumbs { get; set; } = [];
}

