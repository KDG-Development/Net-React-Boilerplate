using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.HeroSlides;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class UpdateHeroSlideRequestValidator : AbstractValidator<UpdateHeroSlideRequest>
{
    public UpdateHeroSlideRequestValidator()
    {
        RuleFor(x => x.ButtonText).NotEmptyWhenProvided();
        RuleFor(x => x.ButtonUrl).NotEmptyWhenProvided().ValidUrlWhenProvided();
    }
}

