using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Abstractions;
public interface IAuthTokenService { (string Token,DateTimeOffset ExpiresAt) CreateAccessToken(UserAccount account); }