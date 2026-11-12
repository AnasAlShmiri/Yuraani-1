using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Identity;
// Development-only. Configure real SMS and email providers before production.
public sealed class DevelopmentVerificationCodeSender(IHostEnvironment env,ILogger<DevelopmentVerificationCodeSender> logger):IVerificationCodeSender
{
 public Task SendAsync(UserAccount account,VerificationChannel channel,string code,CancellationToken ct)
 {
  if(!env.IsDevelopment())throw new InvalidOperationException("Production SMS/email verification providers are not configured.");
  var destination=channel==VerificationChannel.Phone?account.PhoneNumber:account.Email;
  logger.LogWarning("DEV VERIFICATION CODE {Channel} {Destination}: {Code}",channel,destination,code);
  return Task.CompletedTask;
 }
}