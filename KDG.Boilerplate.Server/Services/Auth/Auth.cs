using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KDG.UserManagement.Interfaces;
using KDG.UserManagement.Models;
using KDG.Boilerplate.Server.Models.Users;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public interface IAuthService
{
    string GenerateToken(IUserBase user);
    IUserBase? ValidateToken(string token);
}

public class AuthService : IAuthService 
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    private const string UserClaimKey = "user";

    public static DateTime TokenExpiration()
    {
      return DateTime.UtcNow.AddDays(1);
    }
    public static TimeSpan TokenExpirationSpan()
    {
      return TimeSpan.FromDays(1);
    }

    public AuthService(string secretKey, string issuer, string audience)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(IUserBase user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(UserClaimKey, JsonConvert.SerializeObject(user))
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public IUserBase? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userJson = jwtToken.Claims.First(x => x.Type == UserClaimKey).Value;
            var user = JsonConvert.DeserializeObject<User>(userJson);
            return user;
        }
        catch
        {
            return null;
        }
    }

    public static string? TryFindClaimValue(IEnumerable<Claim> claims, string type) {
      try
      {
        return claims.First(claim => claim.Type == type).Value;
      } catch
      {
        Console.WriteLine("unable to find claim " + type);
        return null;
      }
    }
}
