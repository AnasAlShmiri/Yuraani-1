using MediatR;
namespace Yuraani.Application.Features.Identity.Auth.Commands.RegisterAccount;
public sealed record RegisterAccountCommand(string FullName,string PhoneNumber,string Email,string Password):IRequest<Guid>;