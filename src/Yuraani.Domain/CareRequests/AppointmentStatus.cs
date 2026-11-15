namespace Yuraani.Domain.CareRequests;

/// <summary>حالة الموعد.</summary>
public enum AppointmentStatus
{
    Preliminary = 1,
    Confirmed = 2,
    Rescheduled = 3,
    Completed = 4,
    Cancelled = 5
}
