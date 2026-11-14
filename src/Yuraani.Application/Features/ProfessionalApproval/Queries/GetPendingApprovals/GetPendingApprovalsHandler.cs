using MediatR;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;

namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;

/// <summary>معالج استعلام جلب الطلبات المعلّقة - يُرجع DTOs بدلاً من كيانات Domain.</summary>
public sealed class GetPendingApprovalsHandler(IProfessionalApprovalRepository repository)
    : IRequestHandler<GetPendingApprovalsQuery, IReadOnlyList<PendingApprovalDto>>
{
    public async Task<IReadOnlyList<PendingApprovalDto>> Handle(GetPendingApprovalsQuery request, CancellationToken ct)
    {
        var approvals = await repository.GetPendingAsync(ct);
        return approvals.Select(a => new PendingApprovalDto(
            a.Id,
            a.ProviderId,
            a.ProfessionalType.ToString(),
            a.AttemptNumber,
            a.SubmittedAt)).ToList().AsReadOnly();
    }
}
