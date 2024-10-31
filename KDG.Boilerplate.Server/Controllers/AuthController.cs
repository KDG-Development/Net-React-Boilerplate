using KDG.Boilerplate.Middleware.Auth;
using KDG.Boilerplate.Models.DTO;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public AuthController(
        IUserRepository userRepository,
        IAuthService authService
    )
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserAuth loginRequest)
    {
        var user = await _userRepository.UserLogin(loginRequest);

        if (user == null)
        {
            return Unauthorized();
        }

        var token = _authService.GenerateToken(user);
        AuthHandler.Login(this.HttpContext,token);
        return Ok(new { token });
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
