using System.Net;
using System.Text.Json;
using FluentValidation;

namespace MyApp.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Validation error");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            var payload = JsonSerializer.Serialize(new { message = "Validation failed", errors });
            await context.Response.WriteAsync(payload);
        }
        catch (KeyNotFoundException knf)
        {
            _logger.LogWarning(knf, "Not found");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = knf.Message }));
        }
        catch (UnauthorizedAccessException uae)
        {
            _logger.LogWarning(uae, "Unauthorized");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = uae.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "Daxili server xətası baş verdi.",
                detail = ex.Message
            }));
        }
    }
}
