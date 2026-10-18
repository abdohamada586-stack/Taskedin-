using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Taskedin.Domain.Common;

namespace Taskedin.Infrastructure.Middlewares;

// Converts unhandled exceptions into a standard 500 ApiResponse body.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(
                ex,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            // The response has already started; rethrow and let the host handle it.
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new ApiResponse<string>
            {
                Success = false,
                Message = "An unexpected error occurred while processing the request."
            });
        }
    }
}
