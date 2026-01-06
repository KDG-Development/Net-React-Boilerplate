namespace KDG.Boilerplate.Server.Models.Requests.Cart;

public class CheckoutRequest
{
    public List<CheckoutItemRequest> Items { get; set; } = [];
}

public class CheckoutItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

