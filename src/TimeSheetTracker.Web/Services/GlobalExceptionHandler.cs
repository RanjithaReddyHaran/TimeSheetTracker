using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeSheetTracker.Services;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            BusinessRuleException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
        else
            logger.LogWarning("Request rejected with {StatusCode}: {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                500 => "An unexpected error occurred.",
                409 => "The record was changed by another user.",
                _ => "Request could not be completed."
            },
            Detail = status switch
            {
                500 => "Please try again later. Contact support with the trace identifier if the issue continues.",
                409 => "Refresh the page and retry your action.",
                _ => exception.Message
            },
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });
    }
}
