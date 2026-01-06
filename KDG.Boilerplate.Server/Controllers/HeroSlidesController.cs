using KDG.Boilerplate.Server.Models.Requests.HeroSlides;
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
    public async Task<IActionResult> CreateSlide([FromForm] CreateHeroSlideRequest request)
    {
        using var stream = request.Image!.OpenReadStream();
        var slide = await _heroSlidesService.CreateSlideAsync(
            stream,
            request.Image.FileName,
            request.Image.ContentType,
            request);

        _logger.LogInformation("User {UserId} created hero slide {SlideId}", UserId, slide.Id);
        
        return CreatedAtAction(nameof(GetSlide), new { id = slide.Id }, slide);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateSlide(Guid id, [FromForm] UpdateHeroSlideRequest request)
    {
        Stream? imageStream = null;
        string? fileName = null;
        string? contentType = null;

        if (request.Image != null)
        {
            imageStream = request.Image.OpenReadStream();
            fileName = request.Image.FileName;
            contentType = request.Image.ContentType;
        }

        try
        {
            var slide = await _heroSlidesService.UpdateSlideAsync(id, request, imageStream, fileName, contentType);
            if (slide == null)
                return NotFound();

            _logger.LogInformation("User {UserId} updated hero slide {SlideId}", UserId, id);
        
            return Ok(slide);
        }
        finally
        {
            imageStream?.Dispose();
        }
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
    public async Task<IActionResult> ReorderSlides([FromBody] ReorderHeroSlidesRequest request)
    {
        await _heroSlidesService.ReorderSlidesAsync(request.SlideIds);
        
        _logger.LogInformation("User {UserId} reordered {Count} hero slides", UserId, request.SlideIds.Count);
        
        return Ok(new { });
    }
}
