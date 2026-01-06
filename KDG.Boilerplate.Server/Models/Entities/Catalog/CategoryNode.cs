namespace KDG.Boilerplate.Server.Models.Entities.Catalog;

public class CategoryNode
{
    public string Label { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Dictionary<string, CategoryNode>? Children { get; set; }
}

