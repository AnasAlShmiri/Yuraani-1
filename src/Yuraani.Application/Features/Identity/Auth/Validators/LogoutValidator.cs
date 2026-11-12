using FluentValidation;
using Yuraani.Application.Features.Identity.Auth.Commands.Logout;
namespace Yuraani.Application.Features.Identity.Auth.Validators;
public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.UserAccountId).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
    }
}