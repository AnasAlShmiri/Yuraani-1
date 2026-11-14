using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>تقرير طبي يُعدّه الطبيب عقب الجلسة.</summary>
public sealed class MedicalReport : Entity
{
    private MedicalReport() { }

    public Guid CareEncounterId { get; private set; }
    public Guid MedicalRecordId { get; private set; }
    public Guid DoctorProviderId { get; private set; }

    /// <summary>التقييم السريري.</summary>
    public string Assessment { get; private set; } = string.Empty;

    /// <summary>النتائج والملاحظات.</summary>
    public string Findings { get; private set; } = string.Empty;

    /// <summary>التوصيات.</summary>
    public string Recommendations { get; private set; } = string.Empty;

    /// <summary>وقت الإنشاء.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>يُرسل التقرير الطبي.</summary>
    public static MedicalReport SubmitReport(Guid careEncounterId, Guid medicalRecordId, Guid doctorProviderId,
        string assessment, string findings, string recommendations, DateTimeOffset now)
    {
        if (careEncounterId == Guid.Empty) throw new DomainValidationException("معرّف الجلسة مطلوب.");
        if (medicalRecordId == Guid.Empty) throw new DomainValidationException("معرّف السجل الطبي مطلوب.");
        if (doctorProviderId == Guid.Empty) throw new DomainValidationException("معرّف الطبيب مطلوب.");
        if (string.IsNullOrWhiteSpace(assessment)) throw new DomainValidationException("التقييم السريري مطلوب.");

        return new MedicalReport
        {
            Id = Guid.NewGuid(),
            CareEncounterId = careEncounterId,
            MedicalRecordId = medicalRecordId,
            DoctorProviderId = doctorProviderId,
            Assessment = assessment.Trim(),
            Findings = findings?.Trim() ?? string.Empty,
            Recommendations = recommendations?.Trim() ?? string.Empty,
            CreatedAt = now
        };
    }
}
