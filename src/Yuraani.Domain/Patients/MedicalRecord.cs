using Yuraani.Domain.Common;

namespace Yuraani.Domain.Patients;

/// <summary>السجل الطبي الشامل للمريض.</summary>
public sealed class MedicalRecord : AggregateRoot
{
    private readonly List<LabTestResult> _labTestResults = new();

    /// <summary>معرّف المريض.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>فصيلة الدم.</summary>
    public BloodGroup BloodGroup { get; private set; }

    /// <summary>الأمراض المزمنة (نص حر).</summary>
    public string ChronicDiseases { get; private set; } = string.Empty;

    /// <summary>الحساسيات المعروفة (نص حر).</summary>
    public string Allergies { get; private set; } = string.Empty;

    /// <summary>تاريخ إنشاء السجل.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>آخر تحديث للسجل.</summary>
    public DateTimeOffset LastUpdatedAt { get; private set; }

    /// <summary>نتائج الفحوصات المخبرية.</summary>
    public IReadOnlyCollection<LabTestResult> LabTestResults => _labTestResults.AsReadOnly();

    private MedicalRecord() { }

    /// <summary>يُنشئ سجلاً طبياً جديداً للمريض.</summary>
    public static MedicalRecord CreateForPatient(
        Guid patientId,
        BloodGroup bloodGroup = BloodGroup.Unknown,
        string? chronicDiseases = null,
        string? allergies = null)
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException("معرّف المريض مطلوب لفتح السجل الطبي.");

        var now = DateTimeOffset.UtcNow;
        return new MedicalRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            BloodGroup = bloodGroup,
            ChronicDiseases = chronicDiseases?.Trim() ?? string.Empty,
            Allergies = allergies?.Trim() ?? string.Empty,
            CreatedAt = now,
            LastUpdatedAt = now
        };
    }

    /// <summary>يحدّث البيانات الطارئة (فصيلة الدم والحساسيات والأمراض المزمنة).</summary>
    public void UpdateEmergencyData(BloodGroup bloodGroup, string? chronicDiseases, string? allergies)
    {
        BloodGroup = bloodGroup;
        ChronicDiseases = chronicDiseases?.Trim() ?? string.Empty;
        Allergies = allergies?.Trim() ?? string.Empty;
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>يضيف طلب فحص مخبري جديد.</summary>
    public LabTestResult AddLabTestRequest(Guid requestedByDoctorId, string testName, string? instructions)
    {
        var labTest = LabTestResult.Request(Id, requestedByDoctorId, testName, instructions);
        _labTestResults.Add(labTest);
        LastUpdatedAt = DateTimeOffset.UtcNow;
        return labTest;
    }
}
