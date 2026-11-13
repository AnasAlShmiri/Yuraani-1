using FluentValidation;
using Yuraani.Application.Features.Identity.Auth.Commands.ResendVerification;
namespace Yuraani.Application.Features.Identity.Auth.Validators;
public sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator() { RuleFor(x => x.UserAccountId).NotEmpty(); RuleFor(x => x.Channel).IsInEnum(); }
}