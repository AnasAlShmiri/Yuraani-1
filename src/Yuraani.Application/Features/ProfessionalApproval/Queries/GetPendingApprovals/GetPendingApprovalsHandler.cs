using MediatR;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;
using ApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
public sealed class GetPendingApprovalsHandler(IProfessionalApprovalRepository repository)
    : IRequestHandler<GetPendingApprovalsQuery, IReadOnlyList<ApprovalEntity>>
{
    public Task<IReadOnlyList<ApprovalEntity>> Handle(GetPendingApprovalsQuery request, CancellationToken ct) => repository.GetPendingAsync(ct);
}