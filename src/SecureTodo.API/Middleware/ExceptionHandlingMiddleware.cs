using SecureTodo.Shared.Exceptions;
using SecureTodo.Shared.Results;
using System.Net;
using System.Text.Json;

namespace SecureTodo.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed",
                validationEx.Errors
            ),
            UnauthorizedException => (
                (int)HttpStatusCode.Unauthorized,
                exception.Message,
                new List<string> { exception.Message }
            ),
            NotFoundException => (
                (int)HttpStatusCode.NotFound,
                exception.Message,
                new List<string> { exception.Message }
            ),
            BusinessException => (
                (int)HttpStatusCode.BadRequest,
                exception.Message,
                new List<string> { exception.Message }
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An error occurred processing your request",
                new List<string> { exception.Message }
            )
        };

        context.Response.StatusCode = statusCode;

        var response = Result<object>.FailureResult(message, errors);
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
