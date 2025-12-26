namespace KDG.Boilerplate.Server.Models.Catalog;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Src { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

