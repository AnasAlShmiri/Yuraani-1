using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>سجل غير قابل للاستبدال لتوثيق كل إجراء مراجعة أو قرار إداري.</summary>
public sealed class ProfessionalApprovalReview : Entity<Guid>
{
    private ProfessionalApprovalReview() { }

    public Guid ApprovalId { get; private set; }
    public Guid AdminId { get; private set; }
    public ApprovalReviewAction ReviewAction { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ProfessionalApprovalReview Record(Guid approvalId, Guid adminId, ApprovalReviewAction action, string? notes, DateTimeOffset now)
    {
        if (approvalId == Guid.Empty || adminId == Guid.Empty) throw new ArgumentException("Approval and administrator are required.");
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));
        if (notes?.Length > 1000) throw new ArgumentOutOfRangeException(nameof(notes));
        return new ProfessionalApprovalReview
        {
            Id = Guid.NewGuid(), ApprovalId = approvalId, AdminId = adminId,
            ReviewAction = action, Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(), CreatedAt = now
        };
    }
}
