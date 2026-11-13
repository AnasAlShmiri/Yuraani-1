using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Abstractions;
public interface IAuthTokenService
{
    Task<(string Token, DateTimeOffset ExpiresAt)> CreateAccessTokenAsync(UserAccount account, CancellationToken ct);
}