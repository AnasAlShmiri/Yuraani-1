using System.Security.Cryptography;
using System.Text;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Application.Features.Identity.Auth.DTOs;
using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Commands.RefreshSession;
public sealed class RefreshSessionHandler(IAuthRepository repository, IAuthTokenService tokens, ILoginContext context, TimeProvider clock)
    : IRequestHandler<RefreshSessionCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshSessionCommand request, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var oldHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));
        var oldSession = await repository.FindSessionByHashAsync(oldHash, ct);
        if (oldSession is null || !oldSession.IsActive(now))
            throw new UnauthorizedAccessException("Refresh token is invalid, expired, or revoked.");
        var account = await repository.FindByIdAsync(oldSession.UserAccountId, ct);
        if (account is null || account.AccountStatus != AccountStatus.Active)
            throw new UnauthorizedAccessException("Account is not active.");
        var raw = CreateToken();
        var next = Yuraani.Domain.Identity.RefreshSession.Create(account.Id, Hash(raw), now, context.IpAddress, context.UserAgent);
        oldSession.Revoke(now, next.Id);
        await repository.AddSessionAsync(next, ct);
        var access = await tokens.CreateAccessTokenAsync(account, ct);
        await repository.SaveChangesAsync(ct);
        return new AuthResponse(access.Token, access.ExpiresAt, account.Id, account.FullName, account.PhoneNumber, account.Email,
            raw, next.ExpiresAt);
    }
    private static string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}