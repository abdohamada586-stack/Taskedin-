using Microsoft.AspNetCore.Http;

namespace Taskedin.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine("MIDDLEWARE CAUGHT EXCEPTION");

            context.Response.StatusCode = 500;

            await context.Response.WriteAsync("Exception handled");
        }
    }
}