using System;

namespace KDG.Boilerplate.Server.Models.Catalog;

public class Category
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public Category()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
    }

    public Category(
      Guid id, string name,
      Guid? parentId = null,
      string? description = null
    )
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        Description = description;
    }
}

