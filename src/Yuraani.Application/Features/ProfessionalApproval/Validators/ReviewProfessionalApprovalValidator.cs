using FluentValidation;
using Yuraani.Application.Features.ProfessionalApproval.Commands.ReviewProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Validators;
public sealed class ReviewProfessionalApprovalValidator : AbstractValidator<ReviewProfessionalApprovalCommand>
{
    public ReviewProfessionalApprovalValidator()
    {
        RuleFor(x => x.ApprovalId).NotEmpty();
        RuleFor(x => x.AdministratorAccountId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(1000);
        RuleFor(x => x.Reason).NotEmpty().When(x => !x.Approved);
    }
}