using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class EndpointTestingController : ControllerBase
{
    private static Task DelayIfRequested(int? delayMs)
    {
        if (delayMs.HasValue && delayMs.Value > 0)
        {
            return Task.Delay(delayMs.Value);
        }

        return Task.CompletedTask;
    }

    [HttpGet("200")]
    public async Task<IActionResult> Simulate200([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return Ok(new { message = "Test endpoint: OK" });
    }

    [HttpGet("201")]
    public async Task<IActionResult> Simulate201([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return StatusCode(StatusCodes.Status201Created, new { message = "Test endpoint: Created" });
    }

    [HttpGet("400")]
    public async Task<IActionResult> Simulate400([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return BadRequest(new { error = "Test endpoint: Bad Request" });
    }

    [HttpGet("401")]
    public async Task<IActionResult> Simulate401([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Test endpoint: Unauthorized" });
    }

    [HttpGet("403")]
    public async Task<IActionResult> Simulate403([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return StatusCode(StatusCodes.Status403Forbidden, new { error = "Test endpoint: Forbidden" });
    }

    [HttpGet("404")]
    public async Task<IActionResult> Simulate404([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return NotFound(new { error = "Test endpoint: Not Found" });
    }

    [HttpGet("500")]
    public async Task<IActionResult> Simulate500([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Test endpoint: Internal Server Error" });
    }

    [HttpGet("exception")]
    public async Task<IActionResult> SimulateUnhandledException([FromQuery] int? delayMs)
    {
        await DelayIfRequested(delayMs);
        throw new InvalidOperationException("Test endpoint: Unhandled exception");
    }
}


