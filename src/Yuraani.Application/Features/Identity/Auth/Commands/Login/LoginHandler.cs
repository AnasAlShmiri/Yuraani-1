using System.Security.Cryptography;
using System.Text;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Application.Features.Identity.Auth.DTOs;
using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Commands.Login;
public sealed class LoginHandler(IAuthRepository repo, ISecurePasswordService passwords, IAuthTokenService tokens,
    ILoginContext context, TimeProvider clock) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var isEmail = request.Identifier.Contains('@');
        var normalized = isEmail ? request.Identifier.Trim().ToUpperInvariant() : UserAccount.NormalizePhone(request.Identifier);
        var account = await repo.FindByIdentifierAsync(normalized, isEmail, ct);
        var now = clock.GetUtcNow();
        if (account is null)
        {
            await Log(null, request.Identifier, false, "Invalid credentials.", now, ct);
            await repo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        if (account.IsLockedOut(now))
        {
            await Log(account.Id, request.Identifier, false, "Temporary lockout.", now, ct);
            await repo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Account is temporarily locked. Try again later.");
        }
        if (!passwords.Verify(account.PasswordHash, request.Password))
        {
            account.RecordFailedLogin(now);
            await Log(account.Id, request.Identifier, false, "Invalid credentials.", now, ct);
            await repo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        if (account.AccountStatus != AccountStatus.Active || !account.IsPhoneVerified || !account.IsEmailVerified)
        {
            await Log(account.Id, request.Identifier, false, "Contacts are not fully verified or account is inactive.", now, ct);
            await repo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Verify both phone and email before signing in.");
        }
        account.RecordSuccessfulLogin(now);
        await Log(account.Id, request.Identifier, true, null, now, ct);
        var rawRefresh = CreateRefreshToken();
        var session = RefreshSession.Create(account.Id, Hash(rawRefresh), now, context.IpAddress, context.UserAgent);
        await repo.AddSessionAsync(session, ct);
        await repo.SaveChangesAsync(ct);
        var access = await tokens.CreateAccessTokenAsync(account, ct);
        return new AuthResponse(access.Token, access.ExpiresAt, account.Id, account.FullName,
            account.PhoneNumber, account.Email, rawRefresh, session.ExpiresAt);
    }
    private Task Log(Guid? id, string identifier, bool success, string? reason, DateTimeOffset now, CancellationToken ct) =>
        repo.AddLoginHistoryAsync(LoginHistory.Record(id, identifier, success, reason, context.IpAddress, context.UserAgent, now), ct);
    private static string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}