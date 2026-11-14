using Yuraani.Domain.Common;

namespace Yuraani.Domain.Patients;

public sealed class MedicalRecord : AggregateRoot
{
    private readonly List<LabTestResult> _labTestResults = new();

    public Guid PatientId { get; private set; }
    public BloodGroup BloodGroup { get; private set; }
    public string ChronicDiseases { get; private set; } = string.Empty;
    public string Allergies { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }

    public IReadOnlyCollection<LabTestResult> LabTestResults => _labTestResults.AsReadOnly();

    private MedicalRecord() { }

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

    public void UpdateEmergencyData(BloodGroup bloodGroup, string? chronicDiseases, string? allergies)
    {
        BloodGroup = bloodGroup;
        ChronicDiseases = chronicDiseases?.Trim() ?? string.Empty;
        Allergies = allergies?.Trim() ?? string.Empty;
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public LabTestResult AddLabTestRequest(Guid requestedByDoctorId, string testName, string? instructions)
    {
        var labTest = LabTestResult.Request(Id, requestedByDoctorId, testName, instructions);
        _labTestResults.Add(labTest);
        LastUpdatedAt = DateTimeOffset.UtcNow;
        return labTest;
    }
}

public sealed class LabTestResult : Entity
{
    public Guid MedicalRecordId { get; private set; }
    public Guid RequestedByDoctorId { get; private set; }
    public string TestName { get; private set; } = string.Empty;
    public string? Instructions { get; private set; }
    public string? ResultFileUrl { get; private set; }
    public string? PatientNotes { get; private set; }
    public string? DoctorReviewNotes { get; private set; }
    public bool IsUploaded { get; private set; }
    public bool IsReviewed { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? UploadedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    private LabTestResult() { }

    internal static LabTestResult Request(Guid medicalRecordId, Guid doctorId, string testName, string? instructions)
    {
        if (string.IsNullOrWhiteSpace(testName) || testName.Length > 200)
            throw new DomainValidationException("اسم الفحص الطبي مطلوب.");

        return new LabTestResult
        {
            Id = Guid.NewGuid(),
            MedicalRecordId = medicalRecordId,
            RequestedByDoctorId = doctorId,
            TestName = testName.Trim(),
            Instructions = instructions?.Trim(),
            IsUploaded = false,
            IsReviewed = false,
            RequestedAt = DateTimeOffset.UtcNow
        };
    }

    public void UploadResult(string resultFileUrl, string? patientNotes)
    {
        if (string.IsNullOrWhiteSpace(resultFileUrl))
            throw new DomainValidationException("رابط أو ملف نتيجة الفحص مطلوب.");

        ResultFileUrl = resultFileUrl.Trim();
        PatientNotes = patientNotes?.Trim();
        IsUploaded = true;
        UploadedAt = DateTimeOffset.UtcNow;
    }

    public void ReviewResult(string doctorReviewNotes)
    {
        if (!IsUploaded)
            throw new BusinessRuleException("لا يمكن مراجعة الفحص قبل رفع النتيجة من المريض.");

        DoctorReviewNotes = doctorReviewNotes?.Trim();
        IsReviewed = true;
        ReviewedAt = DateTimeOffset.UtcNow;
    }
}
