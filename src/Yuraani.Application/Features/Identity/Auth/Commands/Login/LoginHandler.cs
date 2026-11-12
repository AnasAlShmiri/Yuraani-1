using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Application.Features.Identity.Auth.DTOs;
using Yuraani.Domain.Identity;

namespace Yuraani.Application.Features.Identity.Auth.Commands.Login;

public sealed class LoginHandler(
    IAuthRepository repo,
    ISecurePasswordService passwords,
    IAuthTokenService tokens,
    ILoginContext context,
    TimeProvider clock) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var isEmail = request.Identifier.Contains('@');
        var normalized = isEmail
            ? request.Identifier.Trim().ToUpperInvariant()
            : UserAccount.NormalizePhone(request.Identifier);

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
            // تُسجل المحاولة قبل إرجاع الخطأ حتى لا تضيع عند رفض بيانات الدخول.
            account.RecordFailedLogin(now);
            await Log(account.Id, request.Identifier, false, "Invalid credentials.", now, ct);
            await repo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (account.AccountStatus != AccountStatus.Active || !account.IsPhoneVerified || !account.IsEmailVerified)
        {
            await Log(account.Id, request.Identifier, false,
                "Contacts are not fully verified or account is inactive.", now, ct);
            await repo.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Verify both phone and email before signing in.");
        }

        account.RecordSuccessfulLogin(now);
        await Log(account.Id, request.Identifier, true, null, now, ct);
        await repo.SaveChangesAsync(ct);

        var token = tokens.CreateAccessToken(account);
        return new AuthResponse(token.Token, token.ExpiresAt, account.Id, account.FullName, account.PhoneNumber, account.Email);
    }

    // يسجل المحاولات الناجحة والفاشلة مع عنوان الشبكة وبيانات العميل.
    private Task Log(Guid? id, string identifier, bool success, string? reason, DateTimeOffset now, CancellationToken ct) =>
        repo.AddLoginHistoryAsync(
            LoginHistory.Record(id, identifier, success, reason, context.IpAddress, context.UserAgent, now), ct);
}
