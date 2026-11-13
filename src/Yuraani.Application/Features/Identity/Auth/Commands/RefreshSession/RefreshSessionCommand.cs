using MediatR;
using Yuraani.Application.Features.Identity.Auth.DTOs;
namespace Yuraani.Application.Features.Identity.Auth.Commands.RefreshSession;
public sealed record RefreshSessionCommand(string RefreshToken) : IRequest<AuthResponse>;