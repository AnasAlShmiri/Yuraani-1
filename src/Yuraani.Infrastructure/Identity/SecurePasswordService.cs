using Microsoft.AspNetCore.Identity;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Identity;
public sealed class SecurePasswordService:ISecurePasswordService
{
 private readonly PasswordHasher<UserAccount> _hasher=new();
 public string Hash(string password)=>_hasher.HashPassword(null!,password);
 public bool Verify(string hash,string password)=>_hasher.VerifyHashedPassword(null!,hash,password)!=PasswordVerificationResult.Failed;
}