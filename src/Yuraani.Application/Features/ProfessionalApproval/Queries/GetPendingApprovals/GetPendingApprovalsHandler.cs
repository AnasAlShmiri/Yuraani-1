using MediatR;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;
using ProfessionalApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
public sealed class GetPendingApprovalsHandler(IProfessionalApprovalRepository repository)
    : IRequestHandler<GetPendingApprovalsQuery, IReadOnlyList<ProfessionalApprovalEntity>>
{
    public Task<IReadOnlyList<ProfessionalApprovalEntity>> Handle(GetPendingApprovalsQuery request, CancellationToken ct) => repository.GetPendingAsync(ct);
}