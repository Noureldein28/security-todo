namespace SecureTodo.API.Middleware;

/// <summary>
/// Security headers middleware
/// Adds security-related HTTP headers
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self'; img-src 'self' data: https:");

        // X-Frame-Options
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-Content-Type-Options
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-XSS-Protection
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        await _next(context);
    }
}
