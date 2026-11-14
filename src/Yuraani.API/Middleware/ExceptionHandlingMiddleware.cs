using FluentValidation;
using System.Net;
using System.Text.Json;
using Yuraani.Domain.Common;

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

            var (status, title, detail) = ex switch
            {
                ValidationException ve => (400, "Validation failed", ve.Message),
                DomainValidationException dve => (400, "Validation failed", dve.Message),
                NotFoundException nfe => (404, "Resource not found", nfe.Message),
                ForbiddenException fe => (403, "Forbidden", fe.Message),
                BusinessRuleException bre => (422, "Business rule violation", bre.Message),
                ConflictException ce => (409, "Conflict", ce.Message),
                UnauthorizedAccessException => (401, "Unauthorized", "Authentication is required."),
                _ => (500, "Server error", "An unexpected error occurred.")
            };

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { type = "about:blank", title, status, detail }));
        }
    }
}
