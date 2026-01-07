using FluentValidation;
using KDG.Boilerplate.Server.Validation;

namespace KDG.UnitTests.Tests.Validation;

/// <summary>
/// Tests for quantity validation rules shared across cart and checkout flows.
/// 
/// Business context: Quantity validation prevents invalid line items that would
/// corrupt orders, manipulate totals, or create nonsensical inventory records.
/// Zero or negative quantities are never valid for product line items.
/// </summary>
public class QuantityValidationTests
{
    private class QuantityModel { public int Quantity { get; set; } }
    private class QuantityValidator : AbstractValidator<QuantityModel>
    {
        public QuantityValidator() => RuleFor(x => x.Quantity).GreaterThanZero();
    }
    
    private readonly QuantityValidator _validator = new();

    /// <summary>
    /// Validates that zero and negative quantities are rejected.
    /// 
    /// Business context: Zero quantities waste database space and display confusingly.
    /// Negative quantities could be exploited to manipulate order totals or inventory,
    /// representing a security boundary against potential fraud.
    /// 
    /// Real-world scenario: Customer clears quantity field (sends 0) or attacker
    /// crafts request with negative quantity. Both are rejected at API boundary.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Quantity_ZeroOrNegative_FailsValidation(int invalidQuantity)
    {
        var model = new QuantityModel { Quantity = invalidQuantity };

        var result = _validator.Validate(model);

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Validates that positive quantities pass validation.
    /// 
    /// Business context: Legitimate orders have positive quantities. This confirms
    /// the validation rule doesn't over-block valid requests.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Quantity_Positive_PassesValidation(int validQuantity)
    {
        var model = new QuantityModel { Quantity = validQuantity };

        var result = _validator.Validate(model);

        Assert.True(result.IsValid);
    }
}

