using System.Security.Cryptography;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;

namespace Yuraani.Application.Features.Identity.Auth.Commands.ResendVerification;

public sealed class ResendVerificationHandler(
    IAuthRepository repo,
    IVerificationCodeHasher codeHasher,
    IVerificationCodeSender sender,
    TimeProvider clock) : IRequestHandler<ResendVerificationCommand>
{
    public async Task Handle(ResendVerificationCommand request, CancellationToken ct)
    {
        var account = await repo.FindByIdAsync(request.UserAccountId, ct)
            ?? throw new InvalidOperationException("Account not found.");

        var isVerified = request.Channel == VerificationChannel.Phone
            ? account.IsPhoneVerified
            : account.IsEmailVerified;
        if (isVerified)
            throw new InvalidOperationException("This contact is already verified.");

        var now = clock.GetUtcNow();
        var latest = await repo.FindLatestChallengeAsync(request.UserAccountId, request.Channel, ct);

        // يمنع الإرسال المتكرر، ويضع سقفاً لعدد الرموز خلال الساعة.
        if (latest is not null && latest.CreatedAt > now.AddSeconds(-60))
            throw new InvalidOperationException("Please wait 60 seconds before requesting another code.");

        var sentInLastHour = await repo.CountChallengesSinceAsync(
            request.UserAccountId, request.Channel, now.AddHours(-1), ct);
        if (sentInLastHour >= 3)
            throw new InvalidOperationException("Verification resend limit reached. Try again later.");

        await repo.RevokeActiveChallengesAsync(request.UserAccountId, request.Channel, now, ct);

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var challenge = VerificationChallenge.Create(
            account.Id, request.Channel, codeHasher.Hash(code), now);

        await repo.AddChallengeAsync(challenge, ct);
        await repo.SaveChangesAsync(ct);
        await sender.SendAsync(account, request.Channel, code, ct);
    }
}
