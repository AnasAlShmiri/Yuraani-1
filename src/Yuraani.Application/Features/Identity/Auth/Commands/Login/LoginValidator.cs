using FluentValidation;
namespace Yuraani.Application.Features.Identity.Auth.Commands.Login;
public sealed class LoginValidator:AbstractValidator<LoginCommand>
{
 public LoginValidator(){RuleFor(x=>x.Identifier).NotEmpty().MaximumLength(254);RuleFor(x=>x.Password).NotEmpty().MaximumLength(128);}
}