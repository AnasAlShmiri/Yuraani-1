using MediatR;
using Yuraani.Application.Features.Identity.Auth.DTOs;
namespace Yuraani.Application.Features.Identity.Auth.Commands.Login;
public sealed record LoginCommand(string Identifier,string Password):IRequest<AuthResponse>;