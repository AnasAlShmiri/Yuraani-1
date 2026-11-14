using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>ترخيص مستقل لكل تخصص؛ صلاحية الطبيب لا تؤثر في صلاحية التمريض.</summary>
public sealed class ProfessionalLicense : Entity
{
    private ProfessionalLicense() { }

    public Guid ProviderId { get; private set; }
    public ProfessionalType ProfessionalType { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public DateOnly? IssuedAt { get; private set; }
    public DateOnly ExpiresAt { get; private set; }
    public string DocumentPath { get; private set; } = string.Empty;
    public LicenseReviewStatus ReviewStatus { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    public static ProfessionalLicense Submit(Guid providerId, ProfessionalType type, string licenseNumber,
        DateOnly? issuedAt, DateOnly expiresAt, string documentPath)
    {
        if (providerId == Guid.Empty) throw new DomainValidationException("Care provider is required.");
        if (!Enum.IsDefined(type)) throw new DomainValidationException("Professional type is invalid.");
        if (string.IsNullOrWhiteSpace(licenseNumber) || licenseNumber.Trim().Length > 80) throw new DomainValidationException("License number is required and must not exceed 80 characters.");
        if (expiresAt <= (issuedAt ?? DateOnly.MinValue)) throw new DomainValidationException("License expiry must be after issue date.");
        if (string.IsNullOrWhiteSpace(documentPath) || documentPath.Trim().Length > 500) throw new DomainValidationException("A valid license document path is required.");

        return new ProfessionalLicense
        {
            Id = Guid.NewGuid(), ProviderId = providerId, ProfessionalType = type,
            LicenseNumber = licenseNumber.Trim(), IssuedAt = issuedAt, ExpiresAt = expiresAt,
            DocumentPath = documentPath.Trim(), ReviewStatus = LicenseReviewStatus.Pending, IsCurrent = false
        };
    }

    public void Review(bool approved, DateTimeOffset now)
    {
        if (ReviewStatus != LicenseReviewStatus.Pending)
            throw new BusinessRuleException("Only pending licenses can be reviewed.");

        if (approved && ExpiresAt < DateOnly.FromDateTime(now.UtcDateTime))
            throw new BusinessRuleException("An expired license cannot be approved.");

        ReviewStatus = approved ? LicenseReviewStatus.Approved : LicenseReviewStatus.Rejected;
        IsCurrent = approved;
        ReviewedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        if (ReviewStatus != LicenseReviewStatus.Approved) throw new BusinessRuleException("Only approved licenses can be revoked.");
        ReviewStatus = LicenseReviewStatus.Revoked;
        IsCurrent = false;
        ReviewedAt = now;
    }

    public void MarkExpired(DateOnly today)
    {
        if (ReviewStatus == LicenseReviewStatus.Approved && ExpiresAt < today)
        {
            ReviewStatus = LicenseReviewStatus.Expired;
            IsCurrent = false;
        }
    }
}
