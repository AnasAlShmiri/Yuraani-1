using Yuraani.Domain.Common;

namespace Yuraani.Domain.Patients;

/// <summary>طلب فحص مخبري ونتيجته.</summary>
public sealed class LabTestResult : Entity
{
    /// <summary>معرّف السجل الطبي.</summary>
    public Guid MedicalRecordId { get; private set; }

    /// <summary>معرّف الطبيب طالب الفحص.</summary>
    public Guid RequestedByDoctorId { get; private set; }

    /// <summary>اسم الفحص.</summary>
    public string TestName { get; private set; } = string.Empty;

    /// <summary>تعليمات التحضير للفحص.</summary>
    public string? Instructions { get; private set; }

    /// <summary>رابط ملف النتيجة.</summary>
    public string? ResultFileUrl { get; private set; }

    /// <summary>ملاحظات المريض.</summary>
    public string? PatientNotes { get; private set; }

    /// <summary>ملاحظات مراجعة الطبيب.</summary>
    public string? DoctorReviewNotes { get; private set; }

    /// <summary>هل رفع المريض النتيجة؟</summary>
    public bool IsUploaded { get; private set; }

    /// <summary>هل راجع الطبيب النتيجة؟</summary>
    public bool IsReviewed { get; private set; }

    /// <summary>وقت الطلب.</summary>
    public DateTimeOffset RequestedAt { get; private set; }

    /// <summary>وقت رفع النتيجة.</summary>
    public DateTimeOffset? UploadedAt { get; private set; }

    /// <summary>وقت مراجعة الطبيب.</summary>
    public DateTimeOffset? ReviewedAt { get; private set; }

    private LabTestResult() { }

    /// <summary>يُنشئ طلب فحص جديد.</summary>
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

    /// <summary>يرفع المريض نتيجة الفحص.</summary>
    public void UploadResult(string resultFileUrl, string? patientNotes)
    {
        if (string.IsNullOrWhiteSpace(resultFileUrl))
            throw new DomainValidationException("رابط أو ملف نتيجة الفحص مطلوب.");

        ResultFileUrl = resultFileUrl.Trim();
        PatientNotes = patientNotes?.Trim();
        IsUploaded = true;
        UploadedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>يراجع الطبيب نتيجة الفحص.</summary>
    public void ReviewResult(string doctorReviewNotes)
    {
        if (!IsUploaded)
            throw new BusinessRuleException("لا يمكن مراجعة الفحص قبل رفع النتيجة من المريض.");

        DoctorReviewNotes = doctorReviewNotes?.Trim();
        IsReviewed = true;
        ReviewedAt = DateTimeOffset.UtcNow;
    }
}
