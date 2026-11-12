using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;

namespace Yuraani.Infrastructure.Identity;

public sealed class JwtAuthTokenService(IConfiguration config, TimeProvider clock) : IAuthTokenService
{
    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(UserAccount account)
    {
        var secret = config["Jwt:Secret"];
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32 ||
            string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Configure Jwt:Secret (minimum 32 bytes), Jwt:Issuer, and Jwt:Audience.");

        var now = clock.GetUtcNow();
        var expires = now.AddMinutes(15);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        // يحمل الرمز هوية الحساب فقط؛ الصلاحيات ستضاف بعد تنفيذ نظام الأدوار.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, account.FullName),
            new Claim("phone_number", account.PhoneNumber),
            new Claim(JwtRegisteredClaimNames.Email, account.Email)
        };

        var jwt = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            now.UtcDateTime,
            expires.UtcDateTime,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
