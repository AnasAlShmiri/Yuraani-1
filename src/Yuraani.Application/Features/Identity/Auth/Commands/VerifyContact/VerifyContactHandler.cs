using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;

namespace Yuraani.Application.Features.Identity.Auth.Commands.VerifyContact;

public sealed class VerifyContactHandler(
    IAuthRepository repo,
    IVerificationCodeHasher codeHasher,
    TimeProvider clock) : IRequestHandler<VerifyContactCommand>
{
    public async Task Handle(VerifyContactCommand request, CancellationToken ct)
    {
        var challenge = await repo.FindLatestChallengeAsync(request.UserAccountId, request.Channel, ct)
            ?? throw new InvalidOperationException("Verification challenge not found.");
        var account = await repo.FindByIdAsync(request.UserAccountId, ct)
            ?? throw new InvalidOperationException("Account not found.");

        var now = clock.GetUtcNow();
        if (!challenge.IsUsable(now))
            throw new InvalidOperationException("Verification code is invalid, expired, or locked.");

        if (!challenge.Matches(codeHasher.Hash(request.Code)))
        {
            // نحفظ عداد المحاولات حتى عند رفض الرمز، ثم نعيد خطأ التحقق للعميل.
            challenge.RecordFailedAttempt(now);
            await repo.SaveChangesAsync(ct);
            throw new InvalidOperationException("Verification code is invalid or expired.");
        }

        challenge.Consume(now);
        account.MarkVerified(request.Channel, now);
        await repo.SaveChangesAsync(ct);
    }
}
