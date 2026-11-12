using MediatR;
namespace Yuraani.Application.Features.ProfessionalApproval.Commands.SubmitProfessionalApproval;
public sealed record SubmitProfessionalApprovalCommand(Guid UserAccountId, Yuraani.Domain.ProfessionalApproval.ProfessionalType ProfessionalType) : IRequest<Guid>;