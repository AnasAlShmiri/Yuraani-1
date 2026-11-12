using MediatR;
using Yuraani.Domain.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
public sealed record GetPendingApprovalsQuery : IRequest<IReadOnlyList<ProfessionalApproval>>;