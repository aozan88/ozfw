using Microsoft.AspNetCore.Http;

public class ScalarMiddleware
{
    private readonly RequestDelegate _next;

    public ScalarMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Example: Process Scalar-specific logic here
        Console.WriteLine("[Scalar Middleware] Processing request...");

        // Add custom behavior, e.g., monitoring, validation, etc.
        await context.Response.WriteAsync("Scalar Middleware in action!");

        // Continue to the next middleware
        await _next(context);
    }
}
