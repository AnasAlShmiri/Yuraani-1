using FluentValidation;
using Yuraani.Application.Features.Identity.Auth.Commands.RefreshSession;
namespace Yuraani.Application.Features.Identity.Auth.Validators;
public sealed class RefreshSessionValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionValidator() => RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
}