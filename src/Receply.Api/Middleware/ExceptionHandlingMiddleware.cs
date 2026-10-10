using System.Net;
using FluentValidation;

namespace Receply.Api.Middleware;

/// <summary>
/// Maps the exception types Application/Domain handlers already throw to proper HTTP responses,
/// instead of letting them fall through as an unhandled 500 with a raw stack trace in the body -
/// which, before this existed, every KeyNotFoundException/InvalidOperationException/ValidationException
/// across the whole API (not just newer code) was doing.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, "Validation failed.", new
            {
                errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteProblem(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException)
        {
            logger.LogWarning(ex, "Request {Method} {Path} rejected: {Message}", context.Request.Method, context.Request.Path, ex.Message);
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblem(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, HttpStatusCode status, string message, object? extra = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message, extra });
    }
}
