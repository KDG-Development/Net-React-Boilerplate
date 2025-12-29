using System;
using System.Collections.Generic;
using KDG.UserManagement.Interfaces;
using KDG.UserManagement.Models;
using KDG.Boilerplate.Server.Models.Organizations;

namespace KDG.Boilerplate.Server.Models.Users;

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
