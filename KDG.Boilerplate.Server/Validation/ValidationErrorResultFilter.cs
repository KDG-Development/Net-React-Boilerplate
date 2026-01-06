using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KDG.Boilerplate.Server.Validation;

/// <summary>
/// Result filter that intercepts validation error responses and transforms
/// error keys to a consistent, client-friendly format.
/// </summary>
public class ValidationErrorResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is BadRequestObjectResult { Value: ValidationProblemDetails problemDetails })
        {
            var transformedErrors = ErrorKeyTransformer.TransformKeys(problemDetails.Errors);
            
            var transformedProblemDetails = new ValidationProblemDetails(transformedErrors)
            {
                Type = problemDetails.Type,
                Title = problemDetails.Title,
                Status = problemDetails.Status,
                Detail = problemDetails.Detail,
                Instance = problemDetails.Instance
            };

            context.Result = new BadRequestObjectResult(transformedProblemDetails);
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // No action needed after result execution
    }
}

