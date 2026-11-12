using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Yuraani.Application.Features.Identity.Auth.Commands.Login;
using Yuraani.Application.Features.Identity.Auth.Commands.RegisterAccount;
using Yuraani.Application.Features.Identity.Auth.Commands.ResendVerification;
using Yuraani.Application.Features.Identity.Auth.Commands.VerifyContact;

namespace Yuraani.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterAccountCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return Accepted(new
        {
            userAccountId = id,
            status = "PendingVerification",
            message = "Verify both phone and email before signing in."
        });
    }

    [HttpPost("verify-contact")]
    public async Task<IActionResult> VerifyContact(VerifyContactCommand command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return NoContent();
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationCommand command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Accepted();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct) =>
        Ok(await sender.Send(command, ct));
}
