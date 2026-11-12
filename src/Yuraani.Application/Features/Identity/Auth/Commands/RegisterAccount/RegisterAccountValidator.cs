using FluentValidation;
namespace Yuraani.Application.Features.Identity.Auth.Commands.RegisterAccount;
public sealed class RegisterAccountValidator:AbstractValidator<RegisterAccountCommand>
{
 public RegisterAccountValidator()
 {
  RuleFor(x=>x.FullName).NotEmpty().MaximumLength(160);
  RuleFor(x=>x.PhoneNumber).NotEmpty().MaximumLength(32);
  RuleFor(x=>x.Email).NotEmpty().EmailAddress().MaximumLength(254);
  RuleFor(x=>x.Password).NotEmpty().MinimumLength(10).MaximumLength(128).Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.").Matches("[a-z]").WithMessage("Password must contain a lowercase letter.").Matches("[0-9]").WithMessage("Password must contain a digit.");
 }
}