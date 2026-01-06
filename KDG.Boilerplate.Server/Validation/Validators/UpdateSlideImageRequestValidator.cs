using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.HeroSlides;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class UpdateSlideImageRequestValidator : AbstractValidator<UpdateSlideImageRequest>
{
    public UpdateSlideImageRequestValidator()
    {
        RuleFor(x => x.Image).RequiredFile();
    }
}

