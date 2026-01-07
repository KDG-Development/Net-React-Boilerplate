using KDG.Boilerplate.Server.Models.Requests.Cart;
using KDG.Boilerplate.Server.Validation.Validators;

namespace KDG.UnitTests.Tests.Validation;

/// <summary>
/// Tests for cart-specific validation rules.
/// 
/// Business context: Cart validation has different requirements than checkout.
/// An empty cart is valid (customer clearing cart) but invalid quantities are not.
/// Quantity validation is tested separately in QuantityValidationTests.
/// </summary>
public class CartValidationTests
{
    private readonly ReplaceCartRequestValidator _validator = new();

    /// <summary>
    /// Validates that an empty cart replacement is allowed.
    /// 
    /// Business context: Clearing the cart is a valid operation. Customer may
    /// remove all items, and frontend sends empty items array representing
    /// an intentionally empty cart.
    /// 
    /// Real-world scenario: Customer clicks "Clear Cart" or removes items one
    /// by one until empty. The empty state must persist correctly.
    /// </summary>
    [Fact]
    public void ReplaceCart_EmptyItems_IsAllowed()
    {
        var request = new ReplaceCartRequest { Items = new List<CartItemRequest>() };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Validates that cart replacement with valid items passes validation.
    /// 
    /// Business context: Normal cart sync from frontend. This confirms the
    /// validator correctly delegates to child item validation.
    /// </summary>
    [Fact]
    public void ReplaceCart_ValidItems_PassesValidation()
    {
        var request = new ReplaceCartRequest
        {
            Items = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Validates that cart replacement fails if any item has invalid quantity.
    /// 
    /// Business context: All-or-nothing validation protects data integrity.
    /// One bad item in a cart sync would corrupt the entire cart state.
    /// </summary>
    [Fact]
    public void ReplaceCart_InvalidItemQuantity_FailsValidation()
    {
        var request = new ReplaceCartRequest
        {
            Items = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { ProductId = Guid.NewGuid(), Quantity = 0 }
            }
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
