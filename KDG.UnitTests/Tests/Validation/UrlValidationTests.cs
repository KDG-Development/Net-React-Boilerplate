using FluentValidation;
using KDG.Boilerplate.Server.Validation;

namespace KDG.UnitTests.Tests.Validation;

/// <summary>
/// Tests for URL validation rules that prevent broken or malicious links
/// in CMS content like hero slides.
/// 
/// Business context: Marketing content links must work correctly and safely.
/// Broken links hurt conversions. Malicious links (javascript:, file://)
/// pose security risks if admins are tricked into entering them.
/// </summary>
public class UrlValidationTests
{
    private class UrlModel { public string Url { get; set; } = string.Empty; }
    private class OptionalUrlModel { public string? Url { get; set; } }

    private class UrlValidator : AbstractValidator<UrlModel>
    {
        public UrlValidator() => RuleFor(x => x.Url).ValidUrl();
    }

    private class OptionalUrlValidator : AbstractValidator<OptionalUrlModel>
    {
        public OptionalUrlValidator() => RuleFor(x => x.Url).ValidUrlWhenProvided();
    }

    private readonly UrlValidator _requiredValidator = new();
    private readonly OptionalUrlValidator _optionalValidator = new();

    /// <summary>
    /// Validates that valid URL formats are accepted.
    /// 
    /// Business context: Internal relative paths and external absolute URLs
    /// are both valid for hero slide links.
    /// 
    /// Real-world scenario: Marketing creates links to internal pages
    /// ("/products/sale") or external partner sites ("https://partner.com").
    /// </summary>
    [Theory]
    [InlineData("/products/sale")]           // Relative path
    [InlineData("/")]                        // Root path
    [InlineData("https://example.com")]      // HTTPS
    [InlineData("http://legacy-vendor.com")] // HTTP (legacy support)
    public void ValidUrl_AcceptedFormats_PassesValidation(string validUrl)
    {
        var result = _requiredValidator.Validate(new UrlModel { Url = validUrl });

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Validates that dangerous URL schemes are rejected.
    /// 
    /// Business context: javascript: and file:// URLs pose security risks.
    /// javascript: enables XSS attacks. file:// exposes local filesystem paths.
    /// 
    /// Real-world scenario: Attacker tricks admin into entering malicious URL
    /// that would execute code or expose system files. Validation blocks it.
    /// </summary>
    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("file:///etc/passwd")]
    [InlineData("data:text/html,<script>evil()</script>")]
    public void ValidUrl_DangerousSchemes_FailsValidation(string dangerousUrl)
    {
        var result = _requiredValidator.Validate(new UrlModel { Url = dangerousUrl });

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Validates that malformed URLs are rejected.
    /// 
    /// Business context: Malformed URLs result in broken links that frustrate
    /// customers and make marketing content look unprofessional.
    /// 
    /// Real-world scenario: Admin typos URL format, entering "products/sale"
    /// instead of "/products/sale". Validation catches the error.
    /// </summary>
    [Theory]
    [InlineData("not-a-url")]
    [InlineData("products/sale")]  // Missing leading slash
    public void ValidUrl_MalformedFormat_FailsValidation(string malformedUrl)
    {
        var result = _requiredValidator.Validate(new UrlModel { Url = malformedUrl });

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Validates optional URL field behavior.
    /// 
    /// Business context: Update operations may not change the URL field.
    /// Null means "don't change", but if provided, must still be valid.
    /// </summary>
    [Fact]
    public void ValidUrlWhenProvided_NullIsAllowed_InvalidIsRejected()
    {
        // Null is allowed
        var nullResult = _optionalValidator.Validate(new OptionalUrlModel { Url = null });
        Assert.True(nullResult.IsValid);

        // Valid URL is allowed
        var validResult = _optionalValidator.Validate(new OptionalUrlModel { Url = "/new-promo" });
        Assert.True(validResult.IsValid);

        // Invalid URL is still rejected
        var invalidResult = _optionalValidator.Validate(new OptionalUrlModel { Url = "javascript:evil()" });
        Assert.False(invalidResult.IsValid);
    }
}
