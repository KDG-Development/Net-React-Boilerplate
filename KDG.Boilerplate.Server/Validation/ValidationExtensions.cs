using FluentValidation;

namespace KDG.Boilerplate.Server.Validation;

public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a string is not null or empty/whitespace.
    /// </summary>
    public static IRuleBuilderOptions<T, string> RequiredString<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .NotEmpty()
            .WithMessage("{PropertyName} is required");

    /// <summary>
    /// Validates that a string is a valid email address.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .EmailAddress()
            .WithMessage("{PropertyName} must be a valid email address");

    /// <summary>
    /// Validates that a string is not null or empty/whitespace when provided (for nullable strings in PATCH requests).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> NotEmptyWhenProvided<T>(this IRuleBuilder<T, string?> ruleBuilder)
        => ruleBuilder
            .Must(value => value == null || !string.IsNullOrWhiteSpace(value))
            .WithMessage("{PropertyName} cannot be empty when provided");

    /// <summary>
    /// Validates that a file is provided and has content.
    /// </summary>
    public static IRuleBuilderOptions<T, IFormFile?> RequiredFile<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder)
        => ruleBuilder
            .NotNull()
            .WithMessage("{PropertyName} is required")
            .Must(file => file != null && file.Length > 0)
            .WithMessage("{PropertyName} cannot be empty");

    /// <summary>
    /// Validates that a file does not exceed the specified size in bytes.
    /// </summary>
    public static IRuleBuilderOptions<T, IFormFile?> MaxFileSize<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, long maxSizeBytes)
        => ruleBuilder
            .Must(file => file == null || file.Length <= maxSizeBytes)
            .WithMessage($"{{PropertyName}} must not exceed {maxSizeBytes / 1024 / 1024}MB");

    /// <summary>
    /// Validates that a file has one of the allowed content types.
    /// </summary>
    public static IRuleBuilderOptions<T, IFormFile?> AllowedContentTypes<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, params string[] contentTypes)
        => ruleBuilder
            .Must(file => file == null || contentTypes.Contains(file.ContentType))
            .WithMessage($"{{PropertyName}} must be one of: {string.Join(", ", contentTypes)}");

    /// <summary>
    /// Validates that a string is a valid absolute URL (http/https) or relative path starting with /.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .Must(IsValidUrl)
            .WithMessage("{PropertyName} must be a valid URL (e.g., /path or https://example.com)");

    /// <summary>
    /// Validates that a nullable string is a valid URL when provided.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidUrlWhenProvided<T>(this IRuleBuilder<T, string?> ruleBuilder)
        => ruleBuilder
            .Must(url => url == null || IsValidUrl(url))
            .WithMessage("{PropertyName} must be a valid URL (e.g., /path or https://example.com)");

    private static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        
        // Allow relative paths starting with /
        if (url.StartsWith('/')) return true;
        
        // Require valid absolute URI with http/https
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) 
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates that a collection is not empty.
    /// </summary>
    public static IRuleBuilderOptions<T, ICollection<TElement>> NotEmptyCollection<T, TElement>(this IRuleBuilder<T, ICollection<TElement>> ruleBuilder)
        => ruleBuilder
            .NotEmpty()
            .WithMessage("{PropertyName} must contain at least one item");

    /// <summary>
    /// Validates that a number is greater than zero.
    /// </summary>
    public static IRuleBuilderOptions<T, int> GreaterThanZero<T>(this IRuleBuilder<T, int> ruleBuilder)
        => ruleBuilder
            .GreaterThan(0)
            .WithMessage("{PropertyName} must be greater than zero");
}

