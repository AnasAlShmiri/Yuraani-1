using FluentValidation;

namespace Yuraani.Application.Features.Identity.Auth.Commands.ResendVerification;

public sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator()
    {
        RuleFor(x => x.UserAccountId).NotEmpty();
        RuleFor(x => x.Channel).IsInEnum();
    }
}
