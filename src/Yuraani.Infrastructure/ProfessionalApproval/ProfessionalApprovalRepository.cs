using Microsoft.EntityFrameworkCore;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;
using Yuraani.Domain.ProfessionalApproval;
using Yuraani.Infrastructure.Persistence;
namespace Yuraani.Infrastructure.ProfessionalApproval;
public sealed class ProfessionalApprovalRepository(YuraaniDbContext db) : IProfessionalApprovalRepository
{
    public Task<Guid?> FindProviderIdByAccountIdAsync(Guid id, CancellationToken ct) =>
        db.CareProviders.Where(x => x.UserAccountId == id).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
    public async Task<int> GetLatestAttemptAsync(Guid providerId, ProfessionalType type, CancellationToken ct) =>
        await db.ProfessionalApprovals.Where(x => x.ProviderId == providerId && x.ProfessionalType == type)
            .Select(x => (int?)x.AttemptNumber).MaxAsync(ct) ?? 0;
    public Task<bool> HasOpenOrApprovedRequestAsync(Guid providerId, ProfessionalType type, CancellationToken ct) =>
        db.ProfessionalApprovals.AnyAsync(x => x.ProviderId == providerId && x.ProfessionalType == type &&
            (x.ApprovalStatus == ApprovalStatus.Pending || x.ApprovalStatus == ApprovalStatus.Approved), ct);
    public Task<Guid?> FindAdministratorIdByAccountIdAsync(Guid id, CancellationToken ct) =>
        db.Administrators.Where(x => x.UserAccountId == id).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
    public Task<ProfessionalApproval?> FindApprovalAsync(Guid id, CancellationToken ct) =>
        db.ProfessionalApprovals.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddApprovalAsync(ProfessionalApproval approval, CancellationToken ct) => await db.ProfessionalApprovals.AddAsync(approval, ct);
    public async Task AddReviewAsync(ProfessionalApprovalReview review, CancellationToken ct) => await db.ProfessionalApprovalReviews.AddAsync(review, ct);
    public async Task<IReadOnlyList<ProfessionalApproval>> GetPendingAsync(CancellationToken ct) =>
        await db.ProfessionalApprovals.AsNoTracking().Where(x => x.ApprovalStatus == ApprovalStatus.Pending).OrderBy(x => x.SubmittedAt).ToListAsync(ct);
    public async Task<IReadOnlyList<ProfessionalApproval>> GetProviderApprovalsAsync(Guid providerId, CancellationToken ct) =>
        await db.ProfessionalApprovals.AsNoTracking().Where(x => x.ProviderId == providerId).OrderByDescending(x => x.SubmittedAt).ToListAsync(ct);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}