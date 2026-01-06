using FluentValidation;
using KDG.Boilerplate.Server.Validation;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Configuration;

public static class ValidationConfiguration
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Register all validators from the assembly
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Disable ASP.NET's automatic model validation - FluentValidation handles all validation
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Register filters globally via MvcOptions
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<FluentValidationActionFilter>();
            options.Filters.Add<ValidationErrorResultFilter>();
        });

        return services;
    }
}

