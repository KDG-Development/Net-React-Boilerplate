namespace KDG.Boilerplate.Server.Models.Entities.Catalog;

public class CategoryDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<CategoryBreadcrumb> Breadcrumbs { get; set; } = new();
    public List<SubcategoryInfo> Subcategories { get; set; } = new();
}

public class CategoryBreadcrumb
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class SubcategoryInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

