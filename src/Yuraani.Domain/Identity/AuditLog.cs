using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class AuditLog : Entity
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
        if (string.IsNullOrWhiteSpace(action) || action.Length > 100) throw new DomainValidationException("Audit action is required.");
        if (string.IsNullOrWhiteSpace(entityType) || entityType.Length > 100) throw new DomainValidationException("Entity type is required.");
        if (details?.Length > 2000) throw new DomainValidationException("Audit details must not exceed 2000 characters.");
        return new AuditLog { Id = Guid.NewGuid(), ActorUserAccountId = actorId, Action = action.Trim(), EntityType = entityType.Trim(), EntityId = entityId, Details = details, IpAddress = ip.TruncateTo(64), OccurredAt = at };
    }
}
