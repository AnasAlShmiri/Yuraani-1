using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class RefreshSession : Entity
{
    public const int MaxIpAddressLength = 64;
    public const int MaxUserAgentLength = 512;

    private RefreshSession() { }
    public Guid UserAccountId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public static RefreshSession Create(Guid userId, string tokenHash, DateTimeOffset now, string? ip, string? agent)
    {
        if (userId == Guid.Empty) throw new DomainValidationException("User account is required.");
        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != 64) throw new DomainValidationException("A SHA-256 token hash is required.");
        return new RefreshSession { Id = Guid.NewGuid(), UserAccountId = userId, TokenHash = tokenHash, CreatedAt = now,
            ExpiresAt = now.AddDays(30), IpAddress = ip.TruncateTo(MaxIpAddressLength), UserAgent = agent.TruncateTo(MaxUserAgentLength) };
    }
    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
    public void Revoke(DateTimeOffset now, Guid? replacedBy = null)
    {
        if (RevokedAt is not null) throw new BusinessRuleException("Refresh session is already revoked.");
        RevokedAt = now;
        ReplacedBySessionId = replacedBy;
    }
}
