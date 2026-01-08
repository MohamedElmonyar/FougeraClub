using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FougeraClub.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        (int statusCode, string title, string detail) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Argument", exception.Message),

            InvalidOperationException => (StatusCodes.Status400BadRequest, "Operation Failed", exception.Message),

            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),

            DbUpdateException => (StatusCodes.Status409Conflict, "Save Failed", "An error occurred while saving data"),

            _ => (StatusCodes.Status500InternalServerError, "Error Occurred", "Internal Server Error")
        };

        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            httpContext.Response.Redirect($"/Home/Error?msg={detail}");
            return true;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}