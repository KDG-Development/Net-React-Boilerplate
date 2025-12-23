using System;

namespace KDG.Boilerplate.Server.Models.Catalog;

public class Product
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }

    public Product()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        Price = 0;
    }

    public Product(
      Guid id,
      string name,
      decimal price,
      Guid? categoryId = null,
      string? description = null
    )
    {
        Id = id;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
    }
}

