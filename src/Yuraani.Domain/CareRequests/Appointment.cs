using Yuraani.Domain.Common;

namespace Yuraani.Domain.CareRequests;

/// <summary>موعد رعاية محدد بين المريض ومقدم الخدمة.</summary>
public sealed class Appointment : AggregateRoot
{
    /// <summary>معرّف طلب الرعاية.</summary>
    public Guid CareRequestId { get; private set; }

    /// <summary>معرّف سجل الإسناد (اختياري).</summary>
    public Guid? RequestAssignmentId { get; private set; }

    /// <summary>معرّف المريض.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>معرّف مقدم الرعاية.</summary>
    public Guid ProviderId { get; private set; }

    /// <summary>بداية الموعد.</summary>
    public DateTimeOffset ScheduledStart { get; private set; }

    /// <summary>نهاية الموعد.</summary>
    public DateTimeOffset ScheduledEnd { get; private set; }

    /// <summary>حالة الموعد.</summary>
    public AppointmentStatus AppointmentStatus { get; private set; }

    /// <summary>موافقة المريض على كشف سجله الطبي.</summary>
    public bool PatientConsent { get; private set; }

    /// <summary>وقت التأكيد.</summary>
    public DateTimeOffset? ConfirmedAt { get; private set; }

    /// <summary>سبب الإلغاء.</summary>
    public string? CancellationReason { get; private set; }

    private Appointment() { }

    /// <summary>يحجز موعداً جديداً.</summary>
    public static Appointment Book(
        Guid careRequestId,
        Guid? requestAssignmentId,
        Guid patientId,
        Guid providerId,
        DateTimeOffset scheduledStart,
        DateTimeOffset scheduledEnd,
        bool patientConsent)
    {
        if (careRequestId == Guid.Empty || patientId == Guid.Empty || providerId == Guid.Empty)
            throw new DomainValidationException("بيانات الطلب والمريض ومقدم الرعاية مطلوبة لحجز الموعد.");
        if (scheduledEnd <= scheduledStart)
            throw new DomainValidationException("وقت نهاية الموعد يجب أن يكون بعد وقت البداية.");

        return new Appointment
        {
            Id = Guid.NewGuid(),
            CareRequestId = careRequestId,
            RequestAssignmentId = requestAssignmentId,
            PatientId = patientId,
            ProviderId = providerId,
            ScheduledStart = scheduledStart,
            ScheduledEnd = scheduledEnd,
            AppointmentStatus = AppointmentStatus.Preliminary,
            PatientConsent = patientConsent
        };
    }

    /// <summary>يؤكد الموعد.</summary>
    public void ConfirmAppointment()
    {
        if (AppointmentStatus is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new BusinessRuleException("لا يمكن تأكيد موعد ملغى أو مكتمل.");
        AppointmentStatus = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>يعيد جدولة الموعد.</summary>
    public void Reschedule(DateTimeOffset newStart, DateTimeOffset newEnd)
    {
        if (AppointmentStatus is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new BusinessRuleException("لا يمكن إعادة جدولة موعد مكتمل أو ملغى.");
        if (newEnd <= newStart)
            throw new DomainValidationException("توقيت الموعد الجديد غير صالح.");

        ScheduledStart = newStart;
        ScheduledEnd = newEnd;
        AppointmentStatus = AppointmentStatus.Rescheduled;
    }

    /// <summary>يلغي الموعد.</summary>
    public void CancelAppointment(string reason)
    {
        if (AppointmentStatus == AppointmentStatus.Completed)
            throw new BusinessRuleException("لا يمكن إلغاء موعد مكتمل.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainValidationException("سبب إلغاء الموعد مطلوب.");

        CancellationReason = reason.Trim();
        AppointmentStatus = AppointmentStatus.Cancelled;
    }

    /// <summary>يُكمل الموعد.</summary>
    public void MarkCompleted()
    {
        if (AppointmentStatus is not (AppointmentStatus.Confirmed or AppointmentStatus.Rescheduled))
            throw new BusinessRuleException("يجب أن يكون الموعد مؤكداً قبل إكماله.");
        AppointmentStatus = AppointmentStatus.Completed;
    }
}
