using FluentValidation;
using System.Net;
using System.Text.Json;
namespace Yuraani.API.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request failed.");
            if (context.Response.HasStarted) throw;
            var status = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };
            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            var detail = status == 500 ? "An unexpected error occurred." : ex.Message;
            var title = status switch
            {
                400 => "Validation failed",
                401 => "Unauthorized",
                404 => "Resource not found",
                409 => "Request conflict",
                _ => "Server error"
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { type = "about:blank", title, status, detail }));
        }
    }
}