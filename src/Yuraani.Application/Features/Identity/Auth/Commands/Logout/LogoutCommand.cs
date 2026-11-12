using MediatR;
namespace Yuraani.Application.Features.Identity.Auth.Commands.Logout;
public sealed record LogoutCommand(Guid UserAccountId, string RefreshToken) : IRequest;