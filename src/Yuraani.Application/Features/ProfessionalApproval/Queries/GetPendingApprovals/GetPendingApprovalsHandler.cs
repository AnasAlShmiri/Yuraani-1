using MediatR;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;
using Yuraani.Domain.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
public sealed class GetPendingApprovalsHandler(IProfessionalApprovalRepository repository)
    : IRequestHandler<GetPendingApprovalsQuery, IReadOnlyList<ProfessionalApproval>>
{
    public Task<IReadOnlyList<ProfessionalApproval>> Handle(GetPendingApprovalsQuery request, CancellationToken ct) => repository.GetPendingAsync(ct);
}