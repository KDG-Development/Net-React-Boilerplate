using KDG.UserManagement.Interfaces;

namespace KDG.Boilerplate.Models.DTO;

public class UserAuth {
    public string Email { get; set; }
    public string Password { get; set; }

    public UserAuth(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
