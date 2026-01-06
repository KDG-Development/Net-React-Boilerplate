using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.Cart;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Cart cannot be empty");
        RuleForEach(x => x.Items).SetValidator(new CheckoutItemRequestValidator());
    }
}

public class CheckoutItemRequestValidator : AbstractValidator<CheckoutItemRequest>
{
    public CheckoutItemRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThanZero();
    }
}

