using System.Security.Cryptography;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;

namespace Yuraani.Application.Features.Identity.Auth.Commands.RegisterAccount;

public sealed class RegisterAccountHandler(
    IAuthRepository repo,
    ISecurePasswordService passwords,
    IVerificationCodeSender sender,
    IVerificationCodeHasher codeHasher,
    TimeProvider clock) : IRequestHandler<RegisterAccountCommand, Guid>
{
    public async Task<Guid> Handle(RegisterAccountCommand request, CancellationToken ct)
    {
        var phone = UserAccount.NormalizePhone(request.PhoneNumber);
        var email = request.Email.Trim().ToUpperInvariant();

        if (!UserAccount.IsValidInternationalPhone(phone))
            throw new ArgumentException("Phone number must use international E.164 format.");

        if (await repo.PhoneExistsAsync(phone, ct))
            throw new InvalidOperationException("Phone number is already registered.");

        if (await repo.EmailExistsAsync(email, ct))
            throw new InvalidOperationException("Email is already registered.");

        var now = clock.GetUtcNow();
        var account = UserAccount.Register(request.FullName, phone, request.Email, passwords.Hash(request.Password), now);
        var phoneCode = GenerateCode();
        var emailCode = GenerateCode();

        await repo.AddAccountAsync(account, ct);
        await repo.AddChallengeAsync(VerificationChallenge.Create(account.Id, VerificationChannel.Phone, codeHasher.Hash(phoneCode), now), ct);
        await repo.AddChallengeAsync(VerificationChallenge.Create(account.Id, VerificationChannel.Email, codeHasher.Hash(emailCode), now), ct);
        await repo.SaveChangesAsync(ct);

        // الإرسال هنا بعد الحفظ؛ فشل مزود الرسائل يتطلب إعادة إرسال أو Outbox لاحقاً.
        await sender.SendAsync(account, VerificationChannel.Phone, phoneCode, ct);
        await sender.SendAsync(account, VerificationChannel.Email, emailCode, ct);

        return account.Id;
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}
