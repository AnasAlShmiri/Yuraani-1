using Yuraani.Domain.Common;

namespace Yuraani.Domain.CareRequests;

/// <summary>سجل إسناد طلب رعاية لمقدم خدمة.</summary>
public sealed class RequestAssignment : Entity
{
    /// <summary>معرّف طلب الرعاية.</summary>
    public Guid CareRequestId { get; private set; }

    /// <summary>معرّف مقدم الرعاية.</summary>
    public Guid ProviderId { get; private set; }

    /// <summary>حالة الإسناد.</summary>
    public AssignmentStatus Status { get; private set; }

    /// <summary>وقت الإسناد.</summary>
    public DateTimeOffset AssignedAt { get; private set; }

    /// <summary>وقت الرد.</summary>
    public DateTimeOffset? RespondedAt { get; private set; }

    /// <summary>سبب الرفض.</summary>
    public string? DeclineReason { get; private set; }

    private RequestAssignment() { }

    /// <summary>يُنشئ سجل إسناد جديد.</summary>
    internal static RequestAssignment Create(Guid careRequestId, Guid providerId)
    {
        return new RequestAssignment
        {
            Id = Guid.NewGuid(),
            CareRequestId = careRequestId,
            ProviderId = providerId,
            Status = AssignmentStatus.Pending,
            AssignedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>يقبل مقدم الرعاية الإسناد.</summary>
    internal void Accept()
    {
        Status = AssignmentStatus.Accepted;
        RespondedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>يرفض مقدم الرعاية الإسناد.</summary>
    internal void Decline(string? reason)
    {
        Status = AssignmentStatus.Declined;
        DeclineReason = reason?.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
    }
}
