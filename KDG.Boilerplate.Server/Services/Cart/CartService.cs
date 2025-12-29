using KDG.Boilerplate.Server.Models.Cart;

namespace KDG.Boilerplate.Services;

public interface ICartService
{
    Task<List<CartProduct>> GetCartAsync(Guid userId);
    Task ReplaceCartAsync(Guid userId, List<UserCartItem> items);
}

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<List<CartProduct>> GetCartAsync(Guid userId)
    {
        return await _cartRepository.GetCartAsync(userId);
    }

    public async Task ReplaceCartAsync(Guid userId, List<UserCartItem> items)
    {
        await _cartRepository.ReplaceCartAsync(userId, items);
    }
}
