using KDG.Boilerplate.Server.Models.Crm;
using KDG.Boilerplate.Services.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class HeroSlidesController : ApiControllerBase
{
    private readonly IHeroSlidesService _heroSlidesService;
    private readonly ILogger<HeroSlidesController> _logger;

    public HeroSlidesController(IHeroSlidesService heroSlidesService, ILogger<HeroSlidesController> logger)
    {
        _heroSlidesService = heroSlidesService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSlides([FromQuery] bool? isActive = null)
    {
        var filters = new HeroSlideFilters { IsActive = isActive };
        var slides = await _heroSlidesService.GetSlidesAsync(filters);
        return Ok(slides);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSlide(Guid id)
    {
        var slide = await _heroSlidesService.GetSlideByIdAsync(id);
        if (slide == null)
            return NotFound();
        return Ok(slide);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateSlide(
        [FromForm] IFormFile image,
        [FromForm] string buttonText,
        [FromForm] string buttonUrl,
        [FromForm] int sortOrder = 0,
        [FromForm] bool isActive = true)
    {
        if (image == null || image.Length == 0)
            return BadRequest("Image is required");

        if (string.IsNullOrWhiteSpace(buttonText))
            return BadRequest("Button text is required");

        if (string.IsNullOrWhiteSpace(buttonUrl))
            return BadRequest("Button URL is required");

        var dto = new CreateHeroSlideDto
        {
            ButtonText = buttonText,
            ButtonUrl = buttonUrl,
            SortOrder = sortOrder,
            IsActive = isActive
        };

        using var stream = image.OpenReadStream();
        var slide = await _heroSlidesService.CreateSlideAsync(
            stream,
            image.FileName,
            image.ContentType,
            dto);

        _logger.LogInformation("User {UserId} created hero slide {SlideId}", UserId, slide.Id);
        
        return CreatedAtAction(nameof(GetSlide), new { id = slide.Id }, slide);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateSlide(Guid id, [FromBody] UpdateHeroSlideDto dto)
    {
        var slide = await _heroSlidesService.UpdateSlideAsync(id, dto);
        if (slide == null)
            return NotFound();

        _logger.LogInformation("User {UserId} updated hero slide {SlideId}", UserId, id);
        
        return Ok(slide);
    }

    [HttpPut("{id:guid}/image")]
    [Authorize]
    public async Task<IActionResult> UpdateSlideImage(Guid id, [FromForm] IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("Image is required");

        using var stream = image.OpenReadStream();
        var slide = await _heroSlidesService.UpdateSlideImageAsync(
            id,
            stream,
            image.FileName,
            image.ContentType);

        if (slide == null)
            return NotFound();

        _logger.LogInformation("User {UserId} updated image for hero slide {SlideId}", UserId, id);
        
        return Ok(slide);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteSlide(Guid id)
    {
        var deleted = await _heroSlidesService.DeleteSlideAsync(id);
        if (!deleted)
            return NotFound();

        _logger.LogInformation("User {UserId} deleted hero slide {SlideId}", UserId, id);
        
        return Ok(new { });
    }

    [HttpPut("reorder")]
    [Authorize]
    public async Task<IActionResult> ReorderSlides([FromBody] ReorderHeroSlidesDto dto)
    {
        await _heroSlidesService.ReorderSlidesAsync(dto.SlideIds);
        
        _logger.LogInformation("User {UserId} reordered {Count} hero slides", UserId, dto.SlideIds.Count);
        
        return Ok(new { });
    }
}

