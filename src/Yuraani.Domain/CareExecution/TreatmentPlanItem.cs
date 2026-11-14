using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>عنصر في الخطة العلاجية (دواء أو إجراء).</summary>
public sealed class TreatmentPlanItem : Entity
{
    private TreatmentPlanItem() { }

    /// <summary>معرّف الخطة العلاجية.</summary>
    public Guid TreatmentPlanId { get; private set; }

    /// <summary>اسم الدواء أو الإجراء.</summary>
    public string MedicationOrProcedureName { get; private set; } = string.Empty;

    /// <summary>الجرعة.</summary>
    public string Dosage { get; private set; } = string.Empty;

    /// <summary>تعليمات الاستخدام.</summary>
    public string UsageInstructions { get; private set; } = string.Empty;

    /// <summary>حالة العنصر.</summary>
    public PlanItemStatus Status { get; private set; }

    /// <summary>هل يُنفَّذ من قِبل الممرض؟</summary>
    public bool IsExecutedByNurse { get; private set; }

    /// <summary>معرّف الممرض المنفِّذ (اختياري).</summary>
    public Guid? ExecutedByNurseId { get; private set; }

    /// <summary>وقت التنفيذ.</summary>
    public DateTimeOffset? ExecutedAt { get; private set; }

    /// <summary>ملاحظة التنفيذ.</summary>
    public string? ExecutionNote { get; private set; }

    internal static TreatmentPlanItem Create(Guid treatmentPlanId, string medicationOrProcedureName,
        string dosage, string usageInstructions, bool isExecutedByNurse)
    {
        if (treatmentPlanId == Guid.Empty) throw new DomainValidationException("معرّف الخطة مطلوب.");
        if (string.IsNullOrWhiteSpace(medicationOrProcedureName))
            throw new DomainValidationException("اسم الدواء أو الإجراء مطلوب.");
        return new TreatmentPlanItem
        {
            Id = Guid.NewGuid(),
            TreatmentPlanId = treatmentPlanId,
            MedicationOrProcedureName = medicationOrProcedureName.Trim(),
            Dosage = dosage?.Trim() ?? string.Empty,
            UsageInstructions = usageInstructions?.Trim() ?? string.Empty,
            Status = PlanItemStatus.Continuing,
            IsExecutedByNurse = isExecutedByNurse
        };
    }

    internal void UpdateStatus(PlanItemStatus newStatus) => Status = newStatus;

    internal void MarkExecutedByNurse(Guid nurseId, string? executionNote, DateTimeOffset now)
    {
        if (Status == PlanItemStatus.Stopped)
            throw new BusinessRuleException("لا يمكن تنفيذ عنصر موقوف.");
        ExecutedByNurseId = nurseId;
        ExecutionNote = executionNote?.Trim();
        ExecutedAt = now;
    }
}
