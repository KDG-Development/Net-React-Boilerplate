using KDG.Boilerplate.Server.Models.Common;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] PaginationParams pagination,
        [FromQuery] ProductFilterParams filters)
    {
        var products = await _productService.GetProductsAsync(pagination, categoryId, filters);
        return Ok(products);
    }
}

