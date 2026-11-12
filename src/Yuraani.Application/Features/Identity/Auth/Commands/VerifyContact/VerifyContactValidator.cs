using FluentValidation;
namespace Yuraani.Application.Features.Identity.Auth.Commands.VerifyContact;
public sealed class VerifyContactValidator:AbstractValidator<VerifyContactCommand>
{
 public VerifyContactValidator(){RuleFor(x=>x.UserAccountId).NotEmpty();RuleFor(x=>x.Channel).IsInEnum();RuleFor(x=>x.Code).Matches("^[0-9]{6}$");}
}