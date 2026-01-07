using KDG.Boilerplate.Server.Models.Requests.Auth;
using KDG.Boilerplate.Server.Validation.Validators;

namespace KDG.UnitTests.Tests.Validation;

/// <summary>
/// Tests for login validation rules that form the first line of defense
/// for account security.
/// 
/// Business context: Login validation catches malformed requests before
/// they reach the authentication system. This prevents wasted database
/// lookups and provides clear error messages to users.
/// </summary>
public class LoginValidationTests
{
    private readonly LoginRequestValidator _validator = new();

    /// <summary>
    /// Validates that empty or whitespace-only emails are rejected.
    /// 
    /// Business context: Empty emails cannot match any account. Failing fast
    /// avoids pointless database lookups and provides clear user feedback.
    /// 
    /// Real-world scenario: User submits form with empty email field or
    /// accidentally pastes whitespace. Server-side validation catches this.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Login_EmptyOrWhitespaceEmail_FailsValidation(string invalidEmail)
    {
        var request = new LoginRequest { Email = invalidEmail, Password = "validpassword" };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Validates that invalid email formats are rejected.
    /// 
    /// Business context: Invalid formats indicate typos or garbage input.
    /// Catching these early prevents confusing "user not found" errors.
    /// 
    /// Real-world scenario: User types "john.doe@company" (missing TLD) or
    /// "johndoe.company.com" (missing @). Validation provides specific feedback.
    /// </summary>
    [Theory]
    [InlineData("notanemail")]
    [InlineData("no-at-sign.com")]
    [InlineData("@nodomain.com")]
    public void Login_InvalidEmailFormat_FailsValidation(string invalidEmail)
    {
        var request = new LoginRequest { Email = invalidEmail, Password = "validpassword" };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Validates that empty or whitespace-only passwords are rejected.
    /// 
    /// Business context: Empty passwords cannot match any account and would
    /// waste BCrypt comparison time. Fast-fail provides immediate feedback.
    /// 
    /// Real-world scenario: User forgets password field or accidentally
    /// pastes whitespace. Validation catches this before BCrypt runs.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Login_EmptyOrWhitespacePassword_FailsValidation(string invalidPassword)
    {
        var request = new LoginRequest { Email = "user@example.com", Password = invalidPassword };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Validates that a complete, valid login request passes all validation.
    /// 
    /// Business context: Happy path - well-formed request proceeds to
    /// authentication. Confirms all validation rules work together.
    /// </summary>
    [Fact]
    public void Login_ValidRequest_PassesValidation()
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "securepassword123"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
