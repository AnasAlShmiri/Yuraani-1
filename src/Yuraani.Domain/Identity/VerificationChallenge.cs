using System.Security.Cryptography;
using Yuraani.Domain.Common;

namespace Yuraani.Domain.Identity;

public sealed class VerificationChallenge : Entity<Guid>
{
    public const int MaximumAttempts = 5;

    private VerificationChallenge() { }

    public Guid UserAccountId { get; private set; }
    public VerificationChannel Channel { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public int AttemptCount { get; private set; }

    public static VerificationChallenge Create(Guid accountId, VerificationChannel channel, string hash, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserAccountId = accountId,
            Channel = channel,
            CodeHash = hash,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(10)
        };

    // تنتهي صلاحية الرمز بعد عشر دقائق أو خمس محاولات فاشلة، أيهما أسبق.
    public bool IsUsable(DateTimeOffset now) =>
        ConsumedAt is null && RevokedAt is null && AttemptCount < MaximumAttempts && ExpiresAt > now;

    // مقارنة ثابتة الزمن لتقليل تسريب معلومات الهاش عبر توقيت الاستجابة.
    public bool Matches(string hash)
    {
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(CodeHash),
                Convert.FromHexString(hash));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public void RecordFailedAttempt(DateTimeOffset now)
    {
        if (!IsUsable(now)) throw new InvalidOperationException("Challenge is no longer usable.");
        AttemptCount++;
        if (AttemptCount >= MaximumAttempts) RevokedAt = now;
    }

    public void Consume(DateTimeOffset now)
    {
        if (!IsUsable(now)) throw new InvalidOperationException("Challenge is no longer usable.");
        ConsumedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        if (ConsumedAt is null && RevokedAt is null) RevokedAt = now;
    }
}
