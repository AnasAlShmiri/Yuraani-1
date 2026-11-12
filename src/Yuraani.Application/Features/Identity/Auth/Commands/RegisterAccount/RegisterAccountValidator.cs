using FluentValidation;
using Yuraani.Domain.Identity;

namespace Yuraani.Application.Features.Identity.Auth.Commands.RegisterAccount;

public sealed class RegisterAccountValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(32)
            .Must(value => UserAccount.IsValidInternationalPhone(UserAccount.NormalizePhone(value)))
            .WithMessage("Phone number must be in international E.164 format, for example +9677XXXXXXXX.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(10).MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }
}
