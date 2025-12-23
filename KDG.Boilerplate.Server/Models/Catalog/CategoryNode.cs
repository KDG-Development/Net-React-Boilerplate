namespace KDG.Boilerplate.Server.Models.Catalog;

public class CategoryNode
{
    public string Label { get; set; } = string.Empty;
    public Dictionary<string, CategoryNode>? Children { get; set; }
}

