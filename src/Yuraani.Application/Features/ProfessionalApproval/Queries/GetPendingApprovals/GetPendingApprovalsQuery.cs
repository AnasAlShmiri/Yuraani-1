using MediatR;
using ApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
public sealed record GetPendingApprovalsQuery : IRequest<IReadOnlyList<ApprovalEntity>>;