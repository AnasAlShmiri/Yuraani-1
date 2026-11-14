using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>سجل غير قابل للاستبدال لتوثيق كل إجراء مراجعة أو قرار؛ يُنشأ فقط من داخل ProfessionalApproval.</summary>
public sealed class ProfessionalApprovalReview : Entity
{
    private ProfessionalApprovalReview() { }

    public Guid ApprovalId { get; private set; }
    /// <summary>فارغ في الإجراءات الصادرة من مقدم الرعاية (Submitted / Withdrawn).</summary>
    public Guid? AdminId { get; private set; }
    public ApprovalReviewAction ReviewAction { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    internal static ProfessionalApprovalReview Record(Guid approvalId, Guid? adminId, ApprovalReviewAction action, string? notes, DateTimeOffset now)
    {
        if (approvalId == Guid.Empty) throw new DomainValidationException("Approval is required.");
        if (!Enum.IsDefined(action)) throw new DomainValidationException("Review action is invalid.");
        if (adminId == Guid.Empty) throw new DomainValidationException("Administrator identifier is invalid.");

        var isProviderAction = action is ApprovalReviewAction.Submitted or ApprovalReviewAction.Withdrawn
            or ApprovalReviewAction.LicenseRenewalSubmitted;
        if (!isProviderAction && adminId is null)
            throw new DomainValidationException("An administrator is required for administrative review actions.");
        if (notes?.Length > 1000) throw new DomainValidationException("Review notes must not exceed 1000 characters.");

        return new ProfessionalApprovalReview
        {
            Id = Guid.NewGuid(), ApprovalId = approvalId, AdminId = adminId,
            ReviewAction = action, Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(), CreatedAt = now
        };
    }
}
