using FluentValidation;
using KDG.Boilerplate.Server.Models.Requests.Cart;

namespace KDG.Boilerplate.Server.Validation.Validators;

public class ReplaceCartRequestValidator : AbstractValidator<ReplaceCartRequest>
{
    public ReplaceCartRequestValidator()
    {
        RuleForEach(x => x.Items).SetValidator(new CartItemRequestValidator());
    }
}

public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
{
    public CartItemRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThanZero();
    }
}

