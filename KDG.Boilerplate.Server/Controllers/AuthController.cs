using KDG.Boilerplate.Middleware.Auth;
using KDG.Boilerplate.Server.Models.Requests.Auth;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public AuthController(
        IUserService userService,
        IAuthService authService
    )
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.UserLoginAsync(request);

        if (user == null)
        {
            return Unauthorized();
        }

        var token = _authService.GenerateToken(user);
        AuthHandler.Login(this.HttpContext, token);
        return Ok(token);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Auth()
    {
        return Ok("authorized");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        AuthHandler.Logout(this.HttpContext);
        return Ok();
    }
}
