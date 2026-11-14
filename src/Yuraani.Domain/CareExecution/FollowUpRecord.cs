using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>سجل متابعة حالة المريض.</summary>
public sealed class FollowUpRecord : Entity
{
    private FollowUpRecord() { }

    public Guid MedicalRecordId { get; private set; }
    public Guid TreatmentPlanId { get; private set; }

    /// <summary>تاريخ المتابعة.</summary>
    public DateTimeOffset FollowUpDate { get; private set; }

    /// <summary>هل تحسّنت الحالة؟</summary>
    public bool HasImproved { get; private set; }

    /// <summary>ملاحظات المتابعة.</summary>
    public string Notes { get; private set; } = string.Empty;

    /// <summary>وقت الإنشاء.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>يُنشئ سجل متابعة.</summary>
    public static FollowUpRecord Create(Guid medicalRecordId, Guid treatmentPlanId,
        DateTimeOffset followUpDate, bool hasImproved, string notes, DateTimeOffset now)
    {
        if (medicalRecordId == Guid.Empty) throw new DomainValidationException("معرّف السجل الطبي مطلوب.");
        if (treatmentPlanId == Guid.Empty) throw new DomainValidationException("معرّف الخطة العلاجية مطلوب.");

        return new FollowUpRecord
        {
            Id = Guid.NewGuid(),
            MedicalRecordId = medicalRecordId,
            TreatmentPlanId = treatmentPlanId,
            FollowUpDate = followUpDate,
            HasImproved = hasImproved,
            Notes = notes?.Trim() ?? string.Empty,
            CreatedAt = now
        };
    }
}
