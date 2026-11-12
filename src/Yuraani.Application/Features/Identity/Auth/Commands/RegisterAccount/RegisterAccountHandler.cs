using System.Security.Cryptography;
using System.Text;
using MediatR;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Commands.RegisterAccount;
public sealed class RegisterAccountHandler(IAuthRepository repo,ISecurePasswordService passwords,IVerificationCodeSender sender,TimeProvider clock):IRequestHandler<RegisterAccountCommand,Guid>
{
 public async Task<Guid> Handle(RegisterAccountCommand request,CancellationToken ct)
 {
  var phone=UserAccount.NormalizePhone(request.PhoneNumber);var email=request.Email.Trim().ToUpperInvariant();
  if(await repo.PhoneExistsAsync(phone,ct))throw new InvalidOperationException("Phone number is already registered.");
  if(await repo.EmailExistsAsync(email,ct))throw new InvalidOperationException("Email is already registered.");
  var now=clock.GetUtcNow();var account=UserAccount.Register(request.FullName,request.PhoneNumber,request.Email,passwords.Hash(request.Password),now);
  var phoneCode=Code();var emailCode=Code();
  await repo.AddAccountAsync(account,ct);
  await repo.AddChallengeAsync(VerificationChallenge.Create(account.Id,VerificationChannel.Phone,Hash(phoneCode),now),ct);
  await repo.AddChallengeAsync(VerificationChallenge.Create(account.Id,VerificationChannel.Email,Hash(emailCode),now),ct);
  await repo.SaveChangesAsync(ct);
  await sender.SendAsync(account,VerificationChannel.Phone,phoneCode,ct);
  await sender.SendAsync(account,VerificationChannel.Email,emailCode,ct);
  return account.Id;
 }
 private static string Code()=>RandomNumberGenerator.GetInt32(0,1_000_000).ToString("D6");
 private static string Hash(string code)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}