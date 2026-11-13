using Yuraani.Domain.Identity;
namespace Yuraani.Application.Features.Identity.Auth.Abstractions;
public interface IAuthRepository
{
    Task<bool> PhoneExistsAsync(string phone, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task AddAccountAsync(UserAccount account, CancellationToken ct);
    Task AddChallengeAsync(VerificationChallenge challenge, CancellationToken ct);
    Task AddLoginHistoryAsync(LoginHistory history, CancellationToken ct);
    Task AddSessionAsync(RefreshSession session, CancellationToken ct);
    Task<RefreshSession?> FindSessionByHashAsync(string hash, CancellationToken ct);
    Task<UserAccount?> FindByIdentifierAsync(string identifier, bool isEmail, CancellationToken ct);
    Task<UserAccount?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<VerificationChallenge?> FindLatestChallengeAsync(Guid accountId, VerificationChannel channel, CancellationToken ct);
    Task<int> CountChallengesSinceAsync(Guid accountId, VerificationChannel channel, DateTimeOffset since, CancellationToken ct);
    Task RevokeActiveChallengesAsync(Guid accountId, VerificationChannel channel, DateTimeOffset now, CancellationToken ct);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid accountId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}