using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class RefreshSession : Entity<Guid>
{
    private RefreshSession() { }
    public Guid UserAccountId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public static RefreshSession Create(Guid userId, string tokenHash, DateTimeOffset now, string? ip, string? agent)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User account is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != 64) throw new ArgumentException("A SHA-256 token hash is required.", nameof(tokenHash));
        return new RefreshSession { Id = Guid.NewGuid(), UserAccountId = userId, TokenHash = tokenHash, CreatedAt = now,
            ExpiresAt = now.AddDays(30), IpAddress = ip, UserAgent = agent };
    }
    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
    public void Revoke(DateTimeOffset now, Guid? replacedBy = null)
    {
        if (RevokedAt is not null) throw new InvalidOperationException("Refresh session is already revoked.");
        RevokedAt = now;
        ReplacedBySessionId = replacedBy;
    }
}