using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.Categories;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class GetCategoryByPathRequestValidator : AbstractValidator<GetCategoryByPathRequest>
{
    public GetCategoryByPathRequestValidator()
    {
        RuleFor(x => x.Path).RequiredString();
    }
}

