using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("by-path")]
    public async Task<IActionResult> GetCategoryByPath([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Path is required");
        }

        // URL-decode the path (handles %2F -> /) and normalize
        var decodedPath = Uri.UnescapeDataString(path);
        var normalizedPath = decodedPath.Trim('/');

        var category = await _categoryService.GetCategoryByPathAsync(normalizedPath);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }
}
