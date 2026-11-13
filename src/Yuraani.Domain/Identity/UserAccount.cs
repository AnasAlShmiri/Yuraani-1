using System.Text.RegularExpressions;
using Yuraani.Domain.Common;

namespace Yuraani.Domain.Identity;

public sealed class UserAccount : Entity<Guid>
{
    private static readonly Regex InternationalPhonePattern = new(@"^\+[1-9][0-9]{7,14}$", RegexOptions.Compiled);

    private UserAccount() { }

    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string NormalizedPhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public AccountStatus AccountStatus { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public int ConsecutiveFailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static UserAccount Register(string fullName, string phone, string email, string passwordHash, DateTimeOffset now)
    {
        var normalizedPhone = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(fullName) || !IsValidInternationalPhone(normalizedPhone) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Required account data is invalid.");

        return new UserAccount
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            PhoneNumber = normalizedPhone,
            NormalizedPhoneNumber = normalizedPhone,
            Email = email.Trim(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            PasswordHash = passwordHash,
            AccountStatus = AccountStatus.PendingVerification,
            CreatedAt = now
        };
    }

    // لا يُفعّل الحساب إلا بعد توثيق الهاتف والبريد الإلكتروني معاً.
    public void MarkVerified(VerificationChannel channel, DateTimeOffset now)
    {
        if (channel == VerificationChannel.Phone) IsPhoneVerified = true;
        else if (channel == VerificationChannel.Email) IsEmailVerified = true;
        else throw new ArgumentOutOfRangeException(nameof(channel));

        if (IsPhoneVerified && IsEmailVerified && AccountStatus == AccountStatus.PendingVerification)
            AccountStatus = AccountStatus.Active;

        UpdatedAt = now;
    }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd.HasValue && LockoutEnd.Value > now;

    // يقفل الحساب 15 دقيقة بعد خمس محاولات دخول فاشلة متتالية.
    public void RecordFailedLogin(DateTimeOffset now)
    {
        ConsecutiveFailedLoginAttempts++;
        if (ConsecutiveFailedLoginAttempts >= 5)
        {
            LockoutEnd = now.AddMinutes(15);
            ConsecutiveFailedLoginAttempts = 0;
        }
        UpdatedAt = now;
    }

    public void RecordSuccessfulLogin(DateTimeOffset now)
    {
        ConsecutiveFailedLoginAttempts = 0;
        LockoutEnd = null;
        UpdatedAt = now;
    }

    // يوحّد الرقم الدولي إلى صيغة E.164 مع الحفاظ على علامة +.
    public static string NormalizePhone(string value)
    {
        var trimmed = value.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (trimmed.StartsWith("00", StringComparison.Ordinal)) digits = digits.Length > 2 ? digits[2..] : string.Empty;
        return trimmed.StartsWith("+", StringComparison.Ordinal) || trimmed.StartsWith("00", StringComparison.Ordinal)
            ? $"+{digits}"
            : digits;
    }

    public static bool IsValidInternationalPhone(string value) => InternationalPhonePattern.IsMatch(value);
}
