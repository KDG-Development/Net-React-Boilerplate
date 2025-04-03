namespace KDG.Boilerplate.Middleware.Auth;

using Hangfire.Dashboard;
using KDG.UserManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

public class AuthHandler
{
  public static void Login(HttpContext ctx, string token)
  {
    ctx.Response.Cookies.Append(
      "auth_token_key", // TODO: use const
      token,
      new CookieOptions()
      {
        Expires = DateTime.Now.AddHours(24),
        Secure = true,
      }
    );
  }
  public static void Logout(HttpContext ctx)
  {
    ctx.Response.Cookies.Append(
      "auth_token_key", // TODO: use const
      "",
      new CookieOptions()
      {
        Expires = DateTime.Now.AddHours(-1),
        Secure = true,
      }
    );
  }
}

public class Permission : TypeFilterAttribute
{
  public Permission(string permission) : base(typeof(PermissionRequirementFilter))
  {
    Arguments = new string[] { permission };
  }

  public class PermissionRequirementFilter : IAuthorizationFilter
  {
    readonly string _permission;

    public PermissionRequirementFilter(string permission)
    {
      _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
      string? _user = AuthService.TryFindClaimValue(context.HttpContext.User.Claims, "user");
      if (_user != null)
      {
        string? expiration = AuthService.TryFindClaimValue(context.HttpContext.User.Claims, "exp");
        if (expiration != null)
        {
          try
          {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(expiration)).UtcDateTime;
            if (expirationTime < DateTime.UtcNow)
            {
              context.Result = new UnauthorizedResult();
              return; // Return early after setting UnauthorizedResult
            }
          }
          catch (Exception)
          {
            // If there's an error parsing the expiration, treat it as expired
            context.Result = new UnauthorizedResult();
            return;
          }
        }
        UserBase? deserialized = JsonConvert.DeserializeObject<UserBase>(_user);
        if (deserialized != null && deserialized.Permissions != null)
        {
          if (!deserialized.Permissions.Any(x => x.Permission == _permission))
          {
            context.Result = new ForbidResult();
          }
          else
          {
          }
        }
        else
        {
          AuthHandler.Logout(context.HttpContext);
          context.Result = new UnauthorizedResult();
        }
      }
      else
      {
        AuthHandler.Logout(context.HttpContext);
        context.Result = new UnauthorizedResult();
      }
    }
  }
}