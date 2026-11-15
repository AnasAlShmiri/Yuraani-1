namespace Yuraani.Domain.CareRequests;

/// <summary>حالة طلب الرعاية.</summary>
public enum RequestStatus
{
    Submitted = 1,
    Assigned = 2,
    Accepted = 3,
    Stalled = 4,
    Completed = 5,
    Cancelled = 6
}
