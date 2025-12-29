using KDG.Boilerplate.Server.Models.Catalog;

namespace KDG.Boilerplate.Server.Models.Cart;

public class UserCartItem
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CartProduct
{
    public ProductMeta Product { get; set; } = new();
    public int Quantity { get; set; }
}
