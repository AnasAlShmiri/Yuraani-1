using System.Security.Cryptography;
using System.Text;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
namespace Yuraani.Application.Features.Identity.Auth.Commands.VerifyContact;
public sealed class VerifyContactHandler(IAuthRepository repo,TimeProvider clock):IRequestHandler<VerifyContactCommand>
{
 public async Task Handle(VerifyContactCommand request,CancellationToken ct)
 {
  var challenge=await repo.FindLatestChallengeAsync(request.UserAccountId,request.Channel,ct)??throw new InvalidOperationException("Verification challenge not found.");
  var account=await repo.FindByIdAsync(request.UserAccountId,ct)??throw new InvalidOperationException("Account not found.");
  var now=clock.GetUtcNow();var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Code)));
  if(!challenge.IsUsable(now)||!challenge.Matches(hash))throw new InvalidOperationException("Verification code is invalid or expired.");
  challenge.Consume(now);account.MarkVerified(request.Channel,now);await repo.SaveChangesAsync(ct);
 }
}