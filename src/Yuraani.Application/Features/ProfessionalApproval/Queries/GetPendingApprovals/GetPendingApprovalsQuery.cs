using MediatR;
using ProfessionalApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
public sealed record GetPendingApprovalsQuery : IRequest<IReadOnlyList<ProfessionalApprovalEntity>>;