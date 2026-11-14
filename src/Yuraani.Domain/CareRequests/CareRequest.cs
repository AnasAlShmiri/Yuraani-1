using Yuraani.Domain.Common;

namespace Yuraani.Domain.CareRequests;

public enum ServiceType
{
    DoctorHomeVisit = 1,
    VideoConsultation = 2,
    HomeNursingCare = 3
}

public enum SymptomInputChannel { Text = 1, Voice = 2 }

public enum RequestStatus
{
    Submitted = 1,
    Assigned = 2,
    Accepted = 3,
    Stalled = 4,
    Completed = 5,
    Cancelled = 6
}

public enum AssignmentStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Expired = 4
}

public enum AppointmentStatus
{
    Preliminary = 1,
    Confirmed = 2,
    Rescheduled = 3,
    Completed = 4,
    Cancelled = 5
}

public sealed class CareRequest : AggregateRoot
{
    private readonly List<RequestAssignment> _assignments = new();

    public Guid PatientId { get; private set; }
    public Guid SelectedProviderId { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public string SymptomDescription { get; private set; } = string.Empty;
    public SymptomInputChannel InputChannel { get; private set; }
    public string? SuggestedSpecialty { get; private set; }
    public bool PatientConsent { get; private set; }
    public RequestStatus RequestStatus { get; private set; }
    public DateTimeOffset PreferredDate { get; private set; }
    public string? VisitAddress { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<RequestAssignment> Assignments => _assignments.AsReadOnly();

    private CareRequest() { }

    public static CareRequest Submit(
        Guid patientId,
        Guid selectedProviderId,
        ServiceType serviceType,
        string symptomDescription,
        SymptomInputChannel inputChannel,
        DateTimeOffset preferredDate,
        bool patientConsent,
        string? visitAddress = null,
        string? suggestedSpecialty = null)
    {
        if (patientId == Guid.Empty || selectedProviderId == Guid.Empty)
            throw new DomainValidationException("بيانات المريض ومقدم الرعاية مطلوبة.");
        if (string.IsNullOrWhiteSpace(symptomDescription) || symptomDescription.Length > 2000)
            throw new DomainValidationException("وصف الأعراض أو الخدمة المطلوبة إلزامي ولا يتجاوز 2000 حرف.");
        if (serviceType != ServiceType.VideoConsultation && string.IsNullOrWhiteSpace(visitAddress))
            throw new DomainValidationException("عنوان الزيارة المنزلية مطلوب.");

        var request = new CareRequest
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            SelectedProviderId = selectedProviderId,
            ServiceType = serviceType,
            SymptomDescription = symptomDescription.Trim(),
            InputChannel = inputChannel,
            SuggestedSpecialty = suggestedSpecialty?.Trim(),
            PatientConsent = patientConsent,
            RequestStatus = RequestStatus.Assigned,
            PreferredDate = preferredDate,
            VisitAddress = visitAddress?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        request._assignments.Add(RequestAssignment.Create(request.Id, selectedProviderId));
        return request;
    }

    public void GrantRecordConsent() => PatientConsent = true;
    public void RevokeRecordConsent() => PatientConsent = false;

    public void AcceptByProvider(Guid providerId)
    {
        var current = _assignments.LastOrDefault(a => a.ProviderId == providerId && a.Status == AssignmentStatus.Pending)
            ?? throw new BusinessRuleException("لا يوجد إسناد معلّق لمقدم الرعاية هذا.");

        current.Accept();
        RequestStatus = RequestStatus.Accepted;
    }

    public void DeclineByProvider(Guid providerId, string? reason = null)
    {
        var current = _assignments.LastOrDefault(a => a.ProviderId == providerId && a.Status == AssignmentStatus.Pending)
            ?? throw new BusinessRuleException("لا يوجد إسناد معلّق لمقدم الرعاية هذا.");

        current.Decline(reason);
        RequestStatus = RequestStatus.Stalled;
    }

    public void ReassignStalledRequest(Guid newProviderId)
    {
        if (RequestStatus != RequestStatus.Stalled)
            throw new BusinessRuleException("لا يمكن إعادة إسناد طلب غير متعثر.");
        if (newProviderId == Guid.Empty)
            throw new DomainValidationException("مقدم الرعاية الجديد مطلوب.");

        SelectedProviderId = newProviderId;
        _assignments.Add(RequestAssignment.Create(Id, newProviderId));
        RequestStatus = RequestStatus.Assigned;
    }

    public void CancelRequest()
    {
        if (RequestStatus == RequestStatus.Completed)
            throw new BusinessRuleException("لا يمكن إلغاء طلب مكتمل.");
        RequestStatus = RequestStatus.Cancelled;
    }

    public void MarkCompleted()
    {
        if (RequestStatus != RequestStatus.Accepted)
            throw new BusinessRuleException("لا يمكن إغلاق طلب لم يتم قبوله وتنفيذه.");
        RequestStatus = RequestStatus.Completed;
    }
}

public sealed class RequestAssignment : Entity
{
    public Guid CareRequestId { get; private set; }
    public Guid ProviderId { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }
    public string? DeclineReason { get; private set; }

    private RequestAssignment() { }

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

    internal void Accept()
    {
        Status = AssignmentStatus.Accepted;
        RespondedAt = DateTimeOffset.UtcNow;
    }

    internal void Decline(string? reason)
    {
        Status = AssignmentStatus.Declined;
        DeclineReason = reason?.Trim();
        RespondedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class Appointment : AggregateRoot
{
    public Guid CareRequestId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ProviderId { get; private set; }
    public DateTimeOffset ScheduledStart { get; private set; }
    public DateTimeOffset ScheduledEnd { get; private set; }
    public AppointmentStatus AppointmentStatus { get; private set; }
    public bool PatientConsent { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private Appointment() { }

    public static Appointment Book(
        Guid careRequestId,
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
            PatientId = patientId,
            ProviderId = providerId,
            ScheduledStart = scheduledStart,
            ScheduledEnd = scheduledEnd,
            AppointmentStatus = AppointmentStatus.Preliminary,
            PatientConsent = patientConsent
        };
    }

    public void ConfirmAppointment()
    {
        if (AppointmentStatus is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new BusinessRuleException("لا يمكن تأكيد موعد ملغى أو مكتمل.");
        AppointmentStatus = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
    }

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

    public void CancelAppointment(string reason)
    {
        if (AppointmentStatus == AppointmentStatus.Completed)
            throw new BusinessRuleException("لا يمكن إلغاء موعد مكتمل.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainValidationException("سبب إلغاء الموعد مطلوب.");

        CancellationReason = reason.Trim();
        AppointmentStatus = AppointmentStatus.Cancelled;
    }

    public void MarkCompleted()
    {
        if (AppointmentStatus is not (AppointmentStatus.Confirmed or AppointmentStatus.Rescheduled))
            throw new BusinessRuleException("يجب أن يكون الموعد مؤكداً قبل إكماله.");
        AppointmentStatus = AppointmentStatus.Completed;
    }
}
