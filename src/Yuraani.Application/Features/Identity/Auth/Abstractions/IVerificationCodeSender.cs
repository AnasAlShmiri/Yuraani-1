using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Abstractions;
public interface IVerificationCodeSender { Task SendAsync(UserAccount account,VerificationChannel channel,string code,CancellationToken ct); }