using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>جلسة الرعاية الفعلية المرتبطة بموعد.</summary>
public sealed class CareEncounter : AggregateRoot
{
    private CareEncounter() { }

    /// <summary>معرّف الموعد.</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>معرّف السجل الطبي.</summary>
    public Guid MedicalRecordId { get; private set; }

    /// <summary>معرّف مقدم الرعاية.</summary>
    public Guid ProviderId { get; private set; }

    /// <summary>حالة الجلسة.</summary>
    public EncounterStatus EncounterStatus { get; private set; }

    /// <summary>وقت البدء الفعلي.</summary>
    public DateTimeOffset? StartedAt { get; private set; }

    /// <summary>وقت الإغلاق.</summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>ملاحظات الإغلاق.</summary>
    public string? CompletionNotes { get; private set; }

    /// <summary>هل اكتمل التوثيق الطبي؟</summary>
    public bool HasDocumentation { get; private set; }

    /// <summary>بيانات الزيارة المنزلية (اختياري).</summary>
    public HomeVisit? HomeVisit { get; private set; }

    /// <summary>بيانات استشارة الفيديو (اختياري).</summary>
    public VideoConsultation? VideoConsultation { get; private set; }

    /// <summary>يُنشئ جلسة زيارة منزلية.</summary>
    public static CareEncounter CreateHomeVisitEncounter(Guid appointmentId, Guid medicalRecordId, Guid providerId, string visitAddress)
    {
        ValidateIds(appointmentId, medicalRecordId, providerId);
        var encounter = CreateBase(appointmentId, medicalRecordId, providerId);
        encounter.HomeVisit = HomeVisit.Create(encounter.Id, visitAddress);
        return encounter;
    }

    /// <summary>يُنشئ جلسة استشارة فيديو.</summary>
    public static CareEncounter CreateVideoConsultationEncounter(Guid appointmentId, Guid medicalRecordId, Guid providerId, string sessionReference)
    {
        ValidateIds(appointmentId, medicalRecordId, providerId);
        var encounter = CreateBase(appointmentId, medicalRecordId, providerId);
        encounter.VideoConsultation = VideoConsultation.Create(encounter.Id, sessionReference);
        return encounter;
    }

    /// <summary>يبدأ الجلسة.</summary>
    public void StartEncounter(DateTimeOffset now)
    {
        if (EncounterStatus != EncounterStatus.Scheduled)
            throw new BusinessRuleException("لا يمكن بدء الجلسة في حالتها الحالية.");
        EncounterStatus = EncounterStatus.InProgress;
        StartedAt = now;
    }

    /// <summary>يُشير إلى اكتمال التوثيق الطبي.</summary>
    public void MarkDocumentationSubmitted() => HasDocumentation = true;

    /// <summary>يتحقق من اكتمال التوثيق.</summary>
    public bool IsDocumentationComplete() => HasDocumentation;

    /// <summary>يُغلق الجلسة بعد التحقق من اكتمال التوثيق.</summary>
    public void CompleteEncounter(string? notes, DateTimeOffset now)
    {
        if (EncounterStatus != EncounterStatus.InProgress)
            throw new BusinessRuleException("لا يمكن إغلاق جلسة غير جارية.");
        if (!HasDocumentation)
            throw new BusinessRuleException("لا يمكن إغلاق الجلسة قبل اكتمال التوثيق الطبي.");
        EncounterStatus = EncounterStatus.Completed;
        CompletionNotes = notes?.Trim();
        CompletedAt = now;
    }

    private static CareEncounter CreateBase(Guid appointmentId, Guid medicalRecordId, Guid providerId) =>
        new()
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            MedicalRecordId = medicalRecordId,
            ProviderId = providerId,
            EncounterStatus = EncounterStatus.Scheduled,
            HasDocumentation = false
        };

    private static void ValidateIds(Guid appointmentId, Guid medicalRecordId, Guid providerId)
    {
        if (appointmentId == Guid.Empty) throw new DomainValidationException("معرّف الموعد مطلوب.");
        if (medicalRecordId == Guid.Empty) throw new DomainValidationException("معرّف السجل الطبي مطلوب.");
        if (providerId == Guid.Empty) throw new DomainValidationException("معرّف مقدم الرعاية مطلوب.");
    }
}
