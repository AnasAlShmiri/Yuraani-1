using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Identity;
public sealed class JwtAuthTokenService(IConfiguration config, TimeProvider clock, IAuthRepository repository) : IAuthTokenService
{
    public async Task<(string Token, DateTimeOffset ExpiresAt)> CreateAccessTokenAsync(UserAccount account, CancellationToken ct)
    {
        var secret = config["Jwt:Secret"];
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32 || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Configure Jwt:Secret (minimum 32 bytes), Jwt:Issuer, and Jwt:Audience.");
        var now = clock.GetUtcNow();
        var expires = now.AddMinutes(15);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.FullName),
            new("phone_number", account.PhoneNumber),
            new(JwtRegisteredClaimNames.Email, account.Email)
        };
        foreach (var role in await repository.GetRoleNamesAsync(account.Id, ct))
            claims.Add(new Claim(ClaimTypes.Role, role));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var jwt = new JwtSecurityToken(issuer, audience, claims, now.UtcDateTime, expires.UtcDateTime,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}