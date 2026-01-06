using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.HeroSlides;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class ReorderHeroSlidesRequestValidator : AbstractValidator<ReorderHeroSlidesRequest>
{
    public ReorderHeroSlidesRequestValidator()
    {
        RuleFor(x => x.SlideIds).NotEmptyCollection();
    }
}

