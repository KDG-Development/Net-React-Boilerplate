using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.Auth;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).RequiredString().ValidEmail();
        RuleFor(x => x.Password).RequiredString();
    }
}

