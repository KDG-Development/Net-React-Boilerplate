using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class FavoritesController : ApiControllerBase
{
    private readonly IFavoritesService _favoritesService;

    public FavoritesController(IFavoritesService favoritesService)
    {
        _favoritesService = favoritesService;
    }

    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> AddFavorite(Guid productId)
    {
        var result = await _favoritesService.AddFavoriteAsync(UserId, productId);
        if (!result.Success)
            return StatusCode(403, new { error = result.Error });

        return Ok(new { });
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFavorite(Guid productId)
    {
        var result = await _favoritesService.RemoveFavoriteAsync(UserId, productId);
        if (!result.Success)
            return StatusCode(403, new { error = result.Error });

        return Ok(new { });
    }
}

