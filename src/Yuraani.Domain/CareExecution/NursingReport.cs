using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>تقرير تمريضي يُعدّه الممرض عقب الجلسة.</summary>
public sealed class NursingReport : Entity
{
    private NursingReport() { }

    public Guid CareEncounterId { get; private set; }
    public Guid MedicalRecordId { get; private set; }
    public Guid NurseProviderId { get; private set; }

    /// <summary>معرّف الخطة العلاجية (اختياري).</summary>
    public Guid? TreatmentPlanId { get; private set; }

    /// <summary>العلامات الحيوية.</summary>
    public string VitalSigns { get; private set; } = string.Empty;

    /// <summary>الإجراءات المُنفَّذة.</summary>
    public string ProceduresPerformed { get; private set; } = string.Empty;

    /// <summary>ملخص عناصر الخطة المُنفَّذة.</summary>
    public string ExecutedPlanItemsSummary { get; private set; } = string.Empty;

    /// <summary>ملخص عناصر الخطة المتبقية.</summary>
    public string RemainingPlanItemsSummary { get; private set; } = string.Empty;

    /// <summary>الملاحظات والأعراض الظاهرة.</summary>
    public string ObservationsAndSymptoms { get; private set; } = string.Empty;

    /// <summary>استجابة المريض.</summary>
    public string PatientResponse { get; private set; } = string.Empty;

    /// <summary>وقت الإنشاء.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>يُرسل التقرير التمريضي.</summary>
    public static NursingReport SubmitReport(Guid careEncounterId, Guid medicalRecordId, Guid nurseProviderId,
        Guid? treatmentPlanId, string vitalSigns, string proceduresPerformed,
        string executedPlanItemsSummary, string remainingPlanItemsSummary,
        string observationsAndSymptoms, string patientResponse, DateTimeOffset now)
    {
        if (careEncounterId == Guid.Empty) throw new DomainValidationException("معرّف الجلسة مطلوب.");
        if (medicalRecordId == Guid.Empty) throw new DomainValidationException("معرّف السجل الطبي مطلوب.");
        if (nurseProviderId == Guid.Empty) throw new DomainValidationException("معرّف الممرض مطلوب.");

        return new NursingReport
        {
            Id = Guid.NewGuid(),
            CareEncounterId = careEncounterId,
            MedicalRecordId = medicalRecordId,
            NurseProviderId = nurseProviderId,
            TreatmentPlanId = treatmentPlanId,
            VitalSigns = vitalSigns?.Trim() ?? string.Empty,
            ProceduresPerformed = proceduresPerformed?.Trim() ?? string.Empty,
            ExecutedPlanItemsSummary = executedPlanItemsSummary?.Trim() ?? string.Empty,
            RemainingPlanItemsSummary = remainingPlanItemsSummary?.Trim() ?? string.Empty,
            ObservationsAndSymptoms = observationsAndSymptoms?.Trim() ?? string.Empty,
            PatientResponse = patientResponse?.Trim() ?? string.Empty,
            CreatedAt = now
        };
    }
}
