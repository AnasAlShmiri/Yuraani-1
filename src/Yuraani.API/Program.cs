using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Yuraani.API.Middleware;
using Yuraani.Application;
using Yuraani.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// يحد من تكرار طلبات المصادقة لكل عنوان IP قبل الوصول إلى منطق التحقق.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["Secret"];
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];

if (!string.IsNullOrWhiteSpace(secret) &&
    Encoding.UTF8.GetByteCount(secret) >= 32 &&
    !string.IsNullOrWhiteSpace(issuer) &&
    !string.IsNullOrWhiteSpace(audience))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        });
}
else
{
    // يسمح بتشغيل بيئة التطوير قبل إعداد JWT؛ يجب توفير الإعدادات قبل حماية أي Endpoint.
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("JWT settings are required outside Development.");
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
