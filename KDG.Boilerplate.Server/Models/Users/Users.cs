using System;
using System.Collections.Generic;
using KDG.UserManagement.Interfaces;
using KDG.UserManagement.Models;

namespace KDG.Boilerplate.Server.Models.Users;

public class User : UserBase
{
    public string Email { get; set; }

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
