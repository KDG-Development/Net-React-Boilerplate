namespace KDG.Boilerplate.Server.Models.Entities.Cart;

public class UserCartItem
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

