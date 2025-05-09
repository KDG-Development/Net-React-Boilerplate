using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using KDG.Boilerplate.Middleware.Auth;
using KDG.UserManagement.Models;
using Newtonsoft.Json;

namespace KDG.Boilerplate.Server.Tests
{
  public class AuthHandlerTests
  {
    [Fact]
    public void Login_SetsCookieWithToken()
    {
      // Arrange
      var mockContext = new Mock<HttpContext>();
      var mockResponse = new Mock<HttpResponse>();
      var mockCookies = new Mock<IResponseCookies>();
      var token = "test_token";

      mockContext.Setup(c => c.Response).Returns(mockResponse.Object);
      mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

      // Act
      AuthHandler.Login(mockContext.Object, token);

      // Assert
      mockCookies.Verify(c => c.Append(
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
    public void Logout_ClearsCookie()
    {
      // Arrange
      var mockContext = new Mock<HttpContext>();
      var mockResponse = new Mock<HttpResponse>();
      var mockCookies = new Mock<IResponseCookies>();

      mockContext.Setup(c => c.Response).Returns(mockResponse.Object);
      mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

      // Act
      AuthHandler.Logout(mockContext.Object);

      // Assert
      mockCookies.Verify(c => c.Append(
        "auth_token_key",
        "",
        It.Is<CookieOptions>(o => 
          o.Expires < DateTime.Now &&
          o.Secure == true
        )
      ), Times.Once);
    }
  }

  public class PermissionTests
  {
    [Fact]
    public void PermissionRequirementFilter_WithValidPermission_AllowsAccess()
    {
      // Arrange
      var mockHttpContext = new Mock<HttpContext>();
      var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
      var claims = new List<Claim>
      {
        new Claim("user", JsonConvert.SerializeObject(new UserBase 
        { 
          Permissions = new HashSet<PermissionBase> { new PermissionBase("test_permission") } 
        })),
        new Claim("exp", DateTimeOffset.Now.AddHours(1).ToUnixTimeSeconds().ToString())
      };

      mockClaimsPrincipal.Setup(c => c.Claims).Returns(claims);
      mockHttpContext.Setup(c => c.User).Returns(mockClaimsPrincipal.Object);

      var actionDescriptor = new ControllerActionDescriptor
      {
        ControllerName = "TestController",
        ActionName = "TestAction"
      };

      var actionContext = new ActionContext(
        mockHttpContext.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        actionDescriptor
      );
      
      var filterContext = new AuthorizationFilterContext(
        actionContext,
        new List<IFilterMetadata>()
      );

      var filter = new Permission.PermissionRequirementFilter("test_permission");

      // Act
      filter.OnAuthorization(filterContext);

      // Assert
      Assert.Null(filterContext.Result);
    }

    [Fact]
    public void PermissionRequirementFilter_WithInvalidPermission_ReturnsForbid()
    {
      // Arrange
      var mockHttpContext = new Mock<HttpContext>();
      var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
      var claims = new List<Claim>
      {
        new Claim("user", JsonConvert.SerializeObject(new UserBase 
        { 
          Permissions = new HashSet<PermissionBase> { new PermissionBase("different_permission") } 
        })),
        new Claim("exp", DateTimeOffset.Now.AddHours(1).ToUnixTimeSeconds().ToString())
      };

      mockClaimsPrincipal.Setup(c => c.Claims).Returns(claims);
      mockHttpContext.Setup(c => c.User).Returns(mockClaimsPrincipal.Object);

      var actionDescriptor = new ControllerActionDescriptor
      {
        ControllerName = "TestController",
        ActionName = "TestAction"
      };

      var actionContext = new ActionContext(
        mockHttpContext.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        actionDescriptor
      );
      
      var filterContext = new AuthorizationFilterContext(
        actionContext,
        new List<IFilterMetadata>()
      );

      var filter = new Permission.PermissionRequirementFilter("test_permission");

      // Act
      filter.OnAuthorization(filterContext);

      // Assert
      Assert.IsType<ForbidResult>(filterContext.Result);
    }

    [Fact]
    public void PermissionRequirementFilter_WithExpiredToken_ReturnsUnauthorized()
    {
      // Arrange
      var mockHttpContext = new Mock<HttpContext>();
      var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
      var claims = new List<Claim>
      {
        new Claim("user", JsonConvert.SerializeObject(new UserBase 
        { 
          Permissions = new HashSet<PermissionBase> { new PermissionBase("test_permission") } 
        })),
        new Claim("exp", DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds().ToString())
      };

      mockClaimsPrincipal.Setup(c => c.Claims).Returns(claims);
      mockHttpContext.Setup(c => c.User).Returns(mockClaimsPrincipal.Object);

      var actionDescriptor = new ControllerActionDescriptor
      {
        ControllerName = "TestController",
        ActionName = "TestAction"
      };

      var actionContext = new ActionContext(
        mockHttpContext.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        actionDescriptor
      );
      
      var filterContext = new AuthorizationFilterContext(
        actionContext,
        new List<IFilterMetadata>()
      );

      var filter = new Permission.PermissionRequirementFilter("test_permission");

      // Act
      filter.OnAuthorization(filterContext);

      // Assert
      Assert.NotNull(filterContext.Result);
      Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Fact]
    public void PermissionRequirementFilter_WithNoUserClaim_ReturnsUnauthorized()
    {
      // Arrange
      var mockHttpContext = new Mock<HttpContext>();
      var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
      var claims = new List<Claim>();
      var mockResponse = new Mock<HttpResponse>();
      var mockCookies = new Mock<IResponseCookies>();

      mockClaimsPrincipal.Setup(c => c.Claims).Returns(claims);
      mockHttpContext.Setup(c => c.User).Returns(mockClaimsPrincipal.Object);
      mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
      mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

      var actionDescriptor = new ControllerActionDescriptor
      {
        ControllerName = "TestController",
        ActionName = "TestAction"
      };

      var actionContext = new ActionContext(
        mockHttpContext.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        actionDescriptor
      );
      
      var filterContext = new AuthorizationFilterContext(
        actionContext,
        new List<IFilterMetadata>()
      );

      var filter = new Permission.PermissionRequirementFilter("test_permission");

      // Act
      filter.OnAuthorization(filterContext);

      // Assert
      Assert.NotNull(filterContext.Result);
      Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }
  }
} 