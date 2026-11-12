using Yuraani.Domain.Identity;
using Yuraani.Domain.ProfessionalApproval;
using ApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Abstractions;
public interface IProfessionalApprovalRepository
{
    Task<Guid?> FindProviderIdByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<int> GetLatestAttemptAsync(Guid providerId, ProfessionalType type, CancellationToken ct);
    Task<bool> HasOpenOrApprovedRequestAsync(Guid providerId, ProfessionalType type, CancellationToken ct);
    Task<Guid?> FindAdministratorIdByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<ApprovalEntity?> FindApprovalAsync(Guid approvalId, CancellationToken ct);
    Task AddApprovalAsync(ApprovalEntity approval, CancellationToken ct);
    Task AddReviewAsync(ProfessionalApprovalReview review, CancellationToken ct);
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken ct);
    Task<IReadOnlyList<ApprovalEntity>> GetPendingAsync(CancellationToken ct);
    Task<IReadOnlyList<ApprovalEntity>> GetProviderApprovalsAsync(Guid providerId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}