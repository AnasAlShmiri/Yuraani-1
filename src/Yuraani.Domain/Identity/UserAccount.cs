using System.Text.RegularExpressions;
using Yuraani.Domain.Common;

namespace Yuraani.Domain.Identity;

public sealed class UserAccount : Entity
{
    public const int MaxFullNameLength = 160;
    public const int MaxEmailLength = 254;
    public const int MaxSuspensionReasonLength = 500;

    private static readonly Regex InternationalPhonePattern = new(@"^\+[1-9][0-9]{7,14}$", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private UserAccount() { }

    public string FullName { get; private set; } = string.Empty;
    // يُخزَّن الرقم دائماً بصيغة E.164 الموحّدة، لذلك لا حاجة لعمود Normalized منفصل.
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public AccountStatus AccountStatus { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public int ConsecutiveFailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static UserAccount Register(string fullName, string phone, string email, string passwordHash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > MaxFullNameLength)
            throw new DomainValidationException($"الاسم الكامل مطلوب ولا يتجاوز {MaxFullNameLength} حرفاً.");

        var normalizedPhone = NormalizePhone(phone);
        if (!IsValidInternationalPhone(normalizedPhone))
            throw new DomainValidationException("رقم الهاتف يجب أن يكون بالصيغة الدولية E.164.");

        var trimmedEmail = email?.Trim() ?? string.Empty;
        if (!IsValidEmail(trimmedEmail))
            throw new DomainValidationException("صيغة البريد الإلكتروني غير صالحة.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("كلمة المرور المشفّرة مطلوبة.");

        return new UserAccount
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            PhoneNumber = normalizedPhone,
            Email = trimmedEmail,
            NormalizedEmail = trimmedEmail.ToUpperInvariant(),
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
        else throw new DomainValidationException("قناة التحقق غير معروفة.");

        if (IsPhoneVerified && IsEmailVerified && AccountStatus == AccountStatus.PendingVerification)
            AccountStatus = AccountStatus.Active;

        UpdatedAt = now;
    }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd.HasValue && LockoutEnd.Value > now;

    /// <summary>يتحقق من أن حالة الحساب تسمح بتسجيل الدخول (بعد التحقق من كلمة المرور).</summary>
    public void EnsureCanSignIn()
    {
        if (AccountStatus == AccountStatus.Suspended)
            throw new ForbiddenException("الحساب موقوف. يرجى التواصل مع الدعم.", "account_suspended");
        if (AccountStatus == AccountStatus.Closed)
            throw new ForbiddenException("الحساب مغلق.", "account_closed");
        if (AccountStatus != AccountStatus.Active || !IsPhoneVerified || !IsEmailVerified)
            throw new ForbiddenException("يجب توثيق الهاتف والبريد الإلكتروني قبل تسجيل الدخول.", "account_not_verified");
    }

    public void Suspend(string reason, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > MaxSuspensionReasonLength)
            throw new DomainValidationException($"سبب الإيقاف مطلوب ولا يتجاوز {MaxSuspensionReasonLength} حرف.");
        if (AccountStatus == AccountStatus.Closed)
            throw new BusinessRuleException("لا يمكن إيقاف حساب مغلق.");

        AccountStatus = AccountStatus.Suspended;
        SuspensionReason = reason.Trim();
        UpdatedAt = now ?? DateTimeOffset.UtcNow;
    }

    public void Activate(DateTimeOffset? now = null)
    {
        if (AccountStatus == AccountStatus.Closed)
            throw new BusinessRuleException("لا يمكن تفعيل حساب مغلق.");
        if (!IsPhoneVerified || !IsEmailVerified)
            throw new BusinessRuleException("لا يمكن تفعيل الحساب قبل توثيق الهاتف والبريد الإلكتروني.");

        AccountStatus = AccountStatus.Active;
        SuspensionReason = null;
        ConsecutiveFailedLoginAttempts = 0;
        LockoutEnd = null;
        UpdatedAt = now ?? DateTimeOffset.UtcNow;
    }

    public void Close(DateTimeOffset? now = null)
    {
        AccountStatus = AccountStatus.Closed;
        UpdatedAt = now ?? DateTimeOffset.UtcNow;
    }

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
        var trimmed = value?.Trim() ?? string.Empty;
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (trimmed.StartsWith("00", StringComparison.Ordinal)) digits = digits.Length > 2 ? digits[2..] : string.Empty;
        return trimmed.StartsWith("+", StringComparison.Ordinal) || trimmed.StartsWith("00", StringComparison.Ordinal)
            ? $"+{digits}"
            : digits;
    }

    public static bool IsValidInternationalPhone(string value) => InternationalPhonePattern.IsMatch(value);

    public static bool IsValidEmail(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= MaxEmailLength && EmailPattern.IsMatch(value);
}
