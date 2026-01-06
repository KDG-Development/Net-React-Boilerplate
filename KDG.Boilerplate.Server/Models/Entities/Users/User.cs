using KDG.UserManagement.Models;
using KDG.Boilerplate.Server.Models.Entities.Organizations;

namespace KDG.Boilerplate.Server.Models.Entities.Users;

public class User : UserBase
{
    public string Email { get; set; }
    public OrganizationMeta? Organization { get; set; }

    public User() : base()
    {
        Email = string.Empty;
    }

    public User(Guid id) : base(id) 
    {
        Email = string.Empty;
    }

    public User(Guid id, string email) : base(id)
    {
        Email = email;
    }
}

