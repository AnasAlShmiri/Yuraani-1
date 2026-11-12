using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>ملف مقدم الرعاية؛ مستقل عن حساب الدخول ويمكن أن يحمل تخصصي الطبيب والممرض.</summary>
public sealed class CareProvider : Entity<Guid>
{
    private CareProvider() { }

    public Guid UserAccountId { get; private set; }
    public decimal? BasePrice { get; private set; }
    public string? ServiceArea { get; private set; }
    public short? YearsExperience { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static CareProvider Create(Guid userAccountId, DateTimeOffset now)
    {
        if (userAccountId == Guid.Empty) throw new ArgumentException("User account is required.", nameof(userAccountId));
        return new CareProvider { Id = Guid.NewGuid(), UserAccountId = userAccountId, CreatedAt = now };
    }
}
