using Yuraani.Domain.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Abstractions;
public interface IProfessionalApprovalRepository
{
    Task<Guid?> FindProviderIdByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<int> GetLatestAttemptAsync(Guid providerId, ProfessionalType type, CancellationToken ct);
    Task<bool> HasOpenOrApprovedRequestAsync(Guid providerId, ProfessionalType type, CancellationToken ct);
    Task<Guid?> FindAdministratorIdByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<ProfessionalApproval?> FindApprovalAsync(Guid approvalId, CancellationToken ct);
    Task AddApprovalAsync(ProfessionalApproval approval, CancellationToken ct);
    Task AddReviewAsync(ProfessionalApprovalReview review, CancellationToken ct);
    Task<IReadOnlyList<ProfessionalApproval>> GetPendingAsync(CancellationToken ct);
    Task<IReadOnlyList<ProfessionalApproval>> GetProviderApprovalsAsync(Guid providerId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}