using MediatR;
using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Commands.VerifyContact;
public sealed record VerifyContactCommand(Guid UserAccountId,VerificationChannel Channel,string Code):IRequest;