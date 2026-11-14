using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>طلب اعتماد مستقل لكل تخصص، مع حفظ رقم المحاولة وسجل المراجعات داخل الـ Aggregate.</summary>
public sealed class ProfessionalApproval : AggregateRoot
{
    public const int MaxAttempts = 3;
    public const int MaxReasonLength = 1000;

    private readonly List<ProfessionalApprovalReview> _reviews = new();

    private ProfessionalApproval() { }

    public Guid ProviderId { get; private set; }
    public ProfessionalType ProfessionalType { get; private set; }
    public byte AttemptNumber { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }
    public Guid? ReviewedByAdminId { get; private set; }
    public string? DecisionReason { get; private set; }

    public IReadOnlyCollection<ProfessionalApprovalReview> Reviews => _reviews.AsReadOnly();

    public static ProfessionalApproval Submit(Guid providerId, ProfessionalType professionalType, byte attemptNumber, DateTimeOffset now)
    {
        if (providerId == Guid.Empty) throw new DomainValidationException("Care provider is required.");
        if (!Enum.IsDefined(professionalType)) throw new DomainValidationException("Professional type is invalid.");
        if (attemptNumber is < 1 or > MaxAttempts)
            throw new DomainValidationException("A maximum of three submissions is allowed per professional type.");

        var approval = new ProfessionalApproval
        {
            Id = Guid.NewGuid(), ProviderId = providerId, ProfessionalType = professionalType,
            AttemptNumber = attemptNumber, ApprovalStatus = ApprovalStatus.Pending, SubmittedAt = now
        };
        // التقديم يصدر من مقدم الرعاية، لذلك لا يوجد مسؤول إداري في هذا السجل.
        approval._reviews.Add(ProfessionalApprovalReview.Record(approval.Id, null, ApprovalReviewAction.Submitted, null, now));
        return approval;
    }

    public void Decide(Guid administratorId, bool approved, string? reason, DateTimeOffset now)
    {
        if (administratorId == Guid.Empty) throw new DomainValidationException("Reviewing administrator is required.");
        if (ApprovalStatus != ApprovalStatus.Pending) throw new BusinessRuleException("Only pending approval requests can be decided.");
        if (!approved && string.IsNullOrWhiteSpace(reason)) throw new DomainValidationException("A rejection reason is required.");
        if (reason?.Trim().Length > MaxReasonLength) throw new DomainValidationException("Decision reason must not exceed 1000 characters.");

        ApprovalStatus = approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        ReviewedByAdminId = administratorId;
        DecisionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        DecidedAt = now;

        var action = approved ? ApprovalReviewAction.Approved : ApprovalReviewAction.Rejected;
        _reviews.Add(ProfessionalApprovalReview.Record(Id, administratorId, action, DecisionReason, now));
    }

    public void Withdraw(DateTimeOffset now)
    {
        if (ApprovalStatus != ApprovalStatus.Pending) throw new BusinessRuleException("Only pending requests can be withdrawn.");
        ApprovalStatus = ApprovalStatus.Withdrawn;
        DecidedAt = now;
        _reviews.Add(ProfessionalApprovalReview.Record(Id, null, ApprovalReviewAction.Withdrawn, null, now));
    }
}
