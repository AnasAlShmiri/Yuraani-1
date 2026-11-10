namespace Yuraani.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
