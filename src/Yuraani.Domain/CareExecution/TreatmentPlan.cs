using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>الخطة العلاجية الموضوعة من قِبل الطبيب.</summary>
public sealed class TreatmentPlan : AggregateRoot
{
    private readonly List<TreatmentPlanItem> _items = new();
    private TreatmentPlan() { }

    /// <summary>معرّف السجل الطبي.</summary>
    public Guid MedicalRecordId { get; private set; }

    /// <summary>معرّف جلسة الرعاية.</summary>
    public Guid CareEncounterId { get; private set; }

    /// <summary>معرّف الطبيب.</summary>
    public Guid DoctorProviderId { get; private set; }

    /// <summary>ملخص التشخيص.</summary>
    public string DiagnosisSummary { get; private set; } = string.Empty;

    /// <summary>التعليمات العامة.</summary>
    public string GeneralInstructions { get; private set; } = string.Empty;

    /// <summary>هل تتطلب الخطة تنفيذاً تمريضياً؟</summary>
    public bool RequiresNursingExecution { get; private set; }

    /// <summary>وقت الإنشاء.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>آخر تحديث.</summary>
    public DateTimeOffset LastUpdatedAt { get; private set; }

    /// <summary>عناصر الخطة العلاجية.</summary>
    public IReadOnlyCollection<TreatmentPlanItem> Items => _items.AsReadOnly();

    /// <summary>يُنشئ خطة علاجية جديدة.</summary>
    public static TreatmentPlan Create(Guid medicalRecordId, Guid careEncounterId, Guid doctorProviderId,
        string diagnosisSummary, string generalInstructions, bool requiresNursingExecution, DateTimeOffset now)
    {
        if (medicalRecordId == Guid.Empty) throw new DomainValidationException("معرّف السجل الطبي مطلوب.");
        if (careEncounterId == Guid.Empty) throw new DomainValidationException("معرّف الجلسة مطلوب.");
        if (doctorProviderId == Guid.Empty) throw new DomainValidationException("معرّف الطبيب مطلوب.");
        if (string.IsNullOrWhiteSpace(diagnosisSummary))
            throw new DomainValidationException("ملخص التشخيص مطلوب.");

        return new TreatmentPlan
        {
            Id = Guid.NewGuid(),
            MedicalRecordId = medicalRecordId,
            CareEncounterId = careEncounterId,
            DoctorProviderId = doctorProviderId,
            DiagnosisSummary = diagnosisSummary.Trim(),
            GeneralInstructions = generalInstructions?.Trim() ?? string.Empty,
            RequiresNursingExecution = requiresNursingExecution,
            CreatedAt = now,
            LastUpdatedAt = now
        };
    }

    /// <summary>يضيف عنصراً إلى الخطة.</summary>
    public TreatmentPlanItem AddItem(string medicationOrProcedureName, string dosage, string usageInstructions,
        bool isExecutedByNurse, DateTimeOffset now)
    {
        var item = TreatmentPlanItem.Create(Id, medicationOrProcedureName, dosage, usageInstructions, isExecutedByNurse);
        _items.Add(item);
        LastUpdatedAt = now;
        return item;
    }

    /// <summary>يحدّث حالة عنصر الخطة (الطبيب).</summary>
    public void UpdateItemStatusByDoctor(Guid itemId, PlanItemStatus newStatus, DateTimeOffset now)
    {
        var item = _items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new NotFoundException("عنصر الخطة العلاجية غير موجود.");
        item.UpdateStatus(newStatus);
        LastUpdatedAt = now;
    }

    /// <summary>يؤشر الممرض على تنفيذ عنصر.</summary>
    public void MarkItemExecutedByNurse(Guid itemId, Guid nurseId, string? executionNote, DateTimeOffset now)
    {
        var item = _items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new NotFoundException("عنصر الخطة العلاجية غير موجود.");
        item.MarkExecutedByNurse(nurseId, executionNote, now);
        LastUpdatedAt = now;
    }
}
