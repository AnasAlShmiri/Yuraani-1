using Yuraani.Domain.Common;

namespace Yuraani.Domain.CareRequests;

/// <summary>طلب رعاية صحية مقدّم من مريض.</summary>
public sealed class CareRequest : AggregateRoot
{
    private readonly List<RequestAssignment> _assignments = new();

    /// <summary>معرّف المريض.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>معرّف مقدم الرعاية المختار.</summary>
    public Guid SelectedProviderId { get; private set; }

    /// <summary>نوع الخدمة.</summary>
    public ServiceType ServiceType { get; private set; }

    /// <summary>وصف الأعراض.</summary>
    public string SymptomDescription { get; private set; } = string.Empty;

    /// <summary>قناة إدخال الأعراض.</summary>
    public SymptomInputChannel InputChannel { get; private set; }

    /// <summary>التخصص المقترح بعد تحليل الأعراض.</summary>
    public string? SuggestedSpecialty { get; private set; }

    /// <summary>موافقة المريض الصريحة على كشف سجله الطبي.</summary>
    public bool PatientConsent { get; private set; }

    /// <summary>حالة الطلب.</summary>
    public RequestStatus RequestStatus { get; private set; }

    /// <summary>التاريخ المفضّل للزيارة.</summary>
    public DateTimeOffset PreferredDate { get; private set; }

    /// <summary>عنوان الزيارة.</summary>
    public string? VisitAddress { get; private set; }

    /// <summary>وقت تقديم الطلب.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>قائمة الإسنادات.</summary>
    public IReadOnlyCollection<RequestAssignment> Assignments => _assignments.AsReadOnly();

    private CareRequest() { }

    /// <summary>يُنشئ طلب رعاية جديد.</summary>
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

    /// <summary>يمنح المريض موافقته على كشف سجله الطبي.</summary>
    public void GrantRecordConsent() => PatientConsent = true;

    /// <summary>يسحب المريض موافقته على كشف سجله الطبي.</summary>
    public void RevokeRecordConsent() => PatientConsent = false;

    /// <summary>يقبل مقدم الرعاية الطلب.</summary>
    public void AcceptByProvider(Guid providerId)
    {
        var current = _assignments.LastOrDefault(a => a.ProviderId == providerId && a.Status == AssignmentStatus.Pending)
            ?? throw new BusinessRuleException("لا يوجد إسناد معلّق لمقدم الرعاية هذا.");

        current.Accept();
        RequestStatus = RequestStatus.Accepted;
    }

    /// <summary>يرفض مقدم الرعاية الطلب.</summary>
    public void DeclineByProvider(Guid providerId, string? reason = null)
    {
        var current = _assignments.LastOrDefault(a => a.ProviderId == providerId && a.Status == AssignmentStatus.Pending)
            ?? throw new BusinessRuleException("لا يوجد إسناد معلّق لمقدم الرعاية هذا.");

        current.Decline(reason);
        RequestStatus = RequestStatus.Stalled;
    }

    /// <summary>يعيد إسناد طلب متوقف.</summary>
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

    /// <summary>يلغي الطلب.</summary>
    public void CancelRequest()
    {
        if (RequestStatus == RequestStatus.Completed)
            throw new BusinessRuleException("لا يمكن إلغاء طلب مكتمل.");
        RequestStatus = RequestStatus.Cancelled;
    }

    /// <summary>يُكمل الطلب.</summary>
    public void MarkCompleted()
    {
        if (RequestStatus != RequestStatus.Accepted)
            throw new BusinessRuleException("لا يمكن إغلاق طلب لم يتم قبوله وتنفيذه.");
        RequestStatus = RequestStatus.Completed;
    }
}
