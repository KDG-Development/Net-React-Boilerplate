using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KDG.Boilerplate.Server.Controllers;
using KDG.Boilerplate.Models.DTO;
using KDG.Boilerplate.Server.Models.Users;
using KDG.Boilerplate.Services;
using KDG.UserManagement.Interfaces;
using KDG.Boilerplate.Middleware.Auth;

namespace KDG.Boilerplate.Server.Tests
{
  public class AuthControllerTests
  {
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpResponse> _mockHttpResponse;
    private readonly Mock<IResponseCookies> _mockCookies;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
      _mockUserRepository = new Mock<IUserRepository>();
      _mockAuthService = new Mock<IAuthService>();
      _mockHttpContext = new Mock<HttpContext>();
      _mockHttpResponse = new Mock<HttpResponse>();
      _mockCookies = new Mock<IResponseCookies>();
      
      // Setup HttpContext for the controller
      _mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);
      _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockCookies.Object);
      
      _authController = new AuthController(_mockUserRepository.Object, _mockAuthService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = _mockHttpContext.Object
        }
      };
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
      // Arrange
      var loginRequest = new UserAuth("test@example.com", "password");
      var user = new User(Guid.NewGuid(), "test@example.com");
      var token = "test_token";

      _mockUserRepository.Setup(r => r.UserLogin(loginRequest))
        .ReturnsAsync(user);
      _mockAuthService.Setup(s => s.GenerateToken(It.IsAny<IUserBase>()))
        .Returns(token);

      // Act
      var result = await _authController.Login(loginRequest);

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      Assert.NotNull(okResult.Value);
      var response = new AnonymousObject(okResult.Value);
      Assert.Equal(token, response.GetPropertyValue("token"));
      
      // Verify that the cookie was set
      _mockCookies.Verify(c => c.Append(
        "auth_token_key",
        token,
        It.Is<CookieOptions>(o => 
          o.Expires > DateTime.Now && 
          o.Expires <= DateTime.Now.AddHours(24) &&
          o.Secure == true
        )
      ), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
      // Arrange
      var loginRequest = new UserAuth("test@example.com", "wrong_password");

      _mockUserRepository.Setup(r => r.UserLogin(loginRequest))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _authController.Login(loginRequest);

      // Assert
      Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Logout_ReturnsOk()
    {
      // Act
      var result = _authController.Logout();

      // Assert
      Assert.IsType<OkResult>(result);
      
      // Verify that the cookie was cleared
      _mockCookies.Verify(c => c.Append(
        "auth_token_key",
        "",
        It.Is<CookieOptions>(o => 
          o.Expires < DateTime.Now &&
          o.Secure == true
        )
      ), Times.Once);
    }
  }

  // Helper class to access anonymous object properties
  public class AnonymousObject
  {
    private readonly object _object;

    public AnonymousObject(object obj)
    {
      _object = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    public object? GetPropertyValue(string propertyName)
    {
      if (string.IsNullOrEmpty(propertyName))
        throw new ArgumentNullException(nameof(propertyName));

      var property = _object.GetType().GetProperty(propertyName);
      return property?.GetValue(_object);
    }
  }
} 