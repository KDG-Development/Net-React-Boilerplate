namespace KDG.Boilerplate.Server.Models.Requests.Cart;

public class ReplaceCartRequest
{
    public List<CartItemRequest> Items { get; set; } = [];
}

public class CartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

