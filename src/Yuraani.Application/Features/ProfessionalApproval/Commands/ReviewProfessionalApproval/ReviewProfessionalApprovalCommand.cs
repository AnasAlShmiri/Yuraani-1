using MediatR;
namespace Yuraani.Application.Features.ProfessionalApproval.Commands.ReviewProfessionalApproval;
public sealed record ReviewProfessionalApprovalCommand(Guid ApprovalId, Guid AdministratorAccountId, bool Approved, string? Reason) : IRequest;