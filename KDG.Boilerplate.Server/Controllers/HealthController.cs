using KDG.Boilerplate.Middleware.Auth;
using KDG.Boilerplate.Models.DTO;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public HealthController(
        IUserRepository userRepository,
        IAuthService authService
    )
    {
        // we want to register services so that health check includes services health
        _userRepository = userRepository;
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Healthy");
    }
}
