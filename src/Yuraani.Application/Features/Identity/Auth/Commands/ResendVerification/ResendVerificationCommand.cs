using MediatR;
using Yuraani.Domain.Identity;

namespace Yuraani.Application.Features.Identity.Auth.Commands.ResendVerification;

public sealed record ResendVerificationCommand(Guid UserAccountId, VerificationChannel Channel) : IRequest;
