namespace KDG.Boilerplate.Server.Models.Catalog;

/// <summary>
/// Base class containing core product properties shared across all product representations.
/// </summary>
public abstract class ProductCore
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

