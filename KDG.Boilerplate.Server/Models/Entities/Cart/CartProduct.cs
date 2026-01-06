using KDG.Boilerplate.Server.Models.Entities.Catalog;

namespace KDG.Boilerplate.Server.Models.Entities.Cart;

public class CartProduct
{
    public ProductMeta Product { get; set; } = new();
    public int Quantity { get; set; }
}

