using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.HeroSlides;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class CreateHeroSlideRequestValidator : AbstractValidator<CreateHeroSlideRequest>
{
    public CreateHeroSlideRequestValidator()
    {
        RuleFor(x => x.Image).RequiredFile();
        RuleFor(x => x.ButtonText).RequiredString();
        RuleFor(x => x.ButtonUrl).RequiredString().ValidUrl();
    }
}

