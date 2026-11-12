using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class AuditLog : Entity<Guid>
{
    private AuditLog() { }
    public Guid? ActorUserAccountId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public static AuditLog Record(Guid? actorId, string action, string entityType, string? entityId, string? details, string? ip, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(action) || action.Length > 100) throw new ArgumentException("Audit action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType) || entityType.Length > 100) throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (details?.Length > 2000 || ip?.Length > 64) throw new ArgumentOutOfRangeException(nameof(details));
        return new AuditLog { Id = Guid.NewGuid(), ActorUserAccountId = actorId, Action = action.Trim(), EntityType = entityType.Trim(), EntityId = entityId, Details = details, IpAddress = ip, OccurredAt = at };
    }
}