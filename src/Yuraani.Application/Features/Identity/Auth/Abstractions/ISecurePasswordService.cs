namespace Yuraani.Application.Features.Identity.Auth.Abstractions;
public interface ISecurePasswordService { string Hash(string password); bool Verify(string hash,string password); }