using System.Security.Cryptography;
using System.Text;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Commands.Logout;
public sealed class LogoutHandler(IAuthRepository repository, TimeProvider clock) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));
        var session = await repository.FindSessionByHashAsync(hash, ct);
        if (session is null || session.UserAccountId != request.UserAccountId) return;
        if (session.IsActive(clock.GetUtcNow())) session.Revoke(clock.GetUtcNow());
        await repository.SaveChangesAsync(ct);
    }
}