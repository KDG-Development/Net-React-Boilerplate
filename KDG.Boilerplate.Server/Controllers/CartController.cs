using KDG.Boilerplate.Server.Models.Entities.Cart;
using KDG.Boilerplate.Server.Models.Requests.Cart;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(UserId);
        return Ok(cart);
    }

    [HttpPost]
    public async Task<IActionResult> ReplaceCart([FromBody] ReplaceCartRequest request)
    {
        var items = request.Items.Select(i => new UserCartItem
        {
            UserId = UserId,
            ProductId = i.ProductId,
            Quantity = i.Quantity
        }).ToList();

        await _cartService.ReplaceCartAsync(UserId, items);
        var cart = await _cartService.GetCartAsync(UserId);
        return Ok(cart);
    }
}
