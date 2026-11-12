using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>طلب اعتماد مستقل لكل تخصص، مع حفظ رقم المحاولة وسجل القرار.</summary>
public sealed class ProfessionalApproval : Entity<Guid>
{
    private ProfessionalApproval() { }

    public Guid ProviderId { get; private set; }
    public ProfessionalType ProfessionalType { get; private set; }
    public byte AttemptNumber { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }
    public Guid? ReviewedByAdminId { get; private set; }
    public string? DecisionReason { get; private set; }

    public static ProfessionalApproval Submit(Guid providerId, ProfessionalType professionalType, byte attemptNumber, DateTimeOffset now)
    {
        if (providerId == Guid.Empty) throw new ArgumentException("Care provider is required.", nameof(providerId));
        if (!Enum.IsDefined(professionalType)) throw new ArgumentOutOfRangeException(nameof(professionalType));
        if (attemptNumber is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(attemptNumber), "A maximum of three submissions is allowed per professional type.");
        return new ProfessionalApproval
        {
            Id = Guid.NewGuid(), ProviderId = providerId, ProfessionalType = professionalType,
            AttemptNumber = attemptNumber, ApprovalStatus = ApprovalStatus.Pending, SubmittedAt = now
        };
    }

    public void Decide(Guid administratorId, bool approved, string? reason, DateTimeOffset now)
    {
        if (administratorId == Guid.Empty) throw new ArgumentException("Reviewing administrator is required.", nameof(administratorId));
        if (ApprovalStatus != ApprovalStatus.Pending) throw new InvalidOperationException("Only pending approval requests can be decided.");
        if (!approved && string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A rejection reason is required.", nameof(reason));

        ApprovalStatus = approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        ReviewedByAdminId = administratorId;
        DecisionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        DecidedAt = now;
    }

    public void Withdraw(DateTimeOffset now)
    {
        if (ApprovalStatus != ApprovalStatus.Pending) throw new InvalidOperationException("Only pending requests can be withdrawn.");
        ApprovalStatus = ApprovalStatus.Withdrawn;
        DecidedAt = now;
    }
}
