using KDG.Boilerplate.Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KDG.Boilerplate.Server.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            var userClaim = User.Claims.FirstOrDefault(c => c.Type == "user");
            if (userClaim == null)
                throw new UnauthorizedAccessException("User claim not found in token");

            var user = JsonConvert.DeserializeObject<User>(userClaim.Value);
            return user?.Id ?? throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}
