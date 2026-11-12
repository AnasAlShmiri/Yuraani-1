using FluentValidation;
using Yuraani.Application.Features.ProfessionalApproval.Commands.SubmitProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Validators;
public sealed class SubmitProfessionalApprovalValidator : AbstractValidator<SubmitProfessionalApprovalCommand>
{
    public SubmitProfessionalApprovalValidator()
    {
        RuleFor(x => x.UserAccountId).NotEmpty();
        RuleFor(x => x.ProfessionalType).IsInEnum();
    }
}