using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public HealthController(
        IUserService userService,
        IAuthService authService
    )
    {
        // we want to register services so that health check includes services health
        _userService = userService;
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Healthy");
    }
}
