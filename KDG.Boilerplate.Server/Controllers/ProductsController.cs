using KDG.Boilerplate.Server.Models.Common;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class ProductsController : ApiControllerBase
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
        var products = await _productService.GetCatalogProductsAsync(UserId, pagination, categoryId, filters);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _productService.GetCatalogProductByIdAsync(id, UserId);
        if (product == null)
            return NotFound();
        return Ok(product);
    }
}

