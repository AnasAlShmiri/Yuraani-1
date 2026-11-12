using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>ترخيص مستقل لكل تخصص؛ صلاحية الطبيب لا تؤثر في صلاحية التمريض.</summary>
public sealed class ProfessionalLicense : Entity<Guid>
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
        if (providerId == Guid.Empty) throw new ArgumentException("Care provider is required.", nameof(providerId));
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        if (string.IsNullOrWhiteSpace(licenseNumber) || licenseNumber.Trim().Length > 80) throw new ArgumentException("License number is required and must not exceed 80 characters.", nameof(licenseNumber));
        if (expiresAt <= (issuedAt ?? DateOnly.MinValue)) throw new ArgumentException("License expiry must be after issue date.", nameof(expiresAt));
        if (string.IsNullOrWhiteSpace(documentPath) || documentPath.Trim().Length > 500) throw new ArgumentException("A valid license document path is required.", nameof(documentPath));

        return new ProfessionalLicense
        {
            Id = Guid.NewGuid(), ProviderId = providerId, ProfessionalType = type,
            LicenseNumber = licenseNumber.Trim(), IssuedAt = issuedAt, ExpiresAt = expiresAt,
            DocumentPath = documentPath.Trim(), ReviewStatus = LicenseReviewStatus.Pending, IsCurrent = false
        };
    }

    public void Review(bool approved, DateTimeOffset now)
    {
        if (ReviewStatus != LicenseReviewStatus.Pending) throw new InvalidOperationException("Only pending licenses can be reviewed.");
        ReviewStatus = approved ? LicenseReviewStatus.Approved : LicenseReviewStatus.Rejected;
        IsCurrent = approved;
        ReviewedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        if (ReviewStatus != LicenseReviewStatus.Approved) throw new InvalidOperationException("Only approved licenses can be revoked.");
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
