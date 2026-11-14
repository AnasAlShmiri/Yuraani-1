using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>تقييم المريض لجودة الخدمة.</summary>
public sealed class ServiceRating : Entity
{
    private ServiceRating() { }

    public Guid CareEncounterId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ProviderId { get; private set; }

    /// <summary>الدرجة (1-5).</summary>
    public int Score { get; private set; }

    /// <summary>تعليق المريض.</summary>
    public string? Comment { get; private set; }

    /// <summary>وقت التقديم.</summary>
    public DateTimeOffset SubmittedAt { get; private set; }

    /// <summary>يُقدّم التقييم.</summary>
    public static ServiceRating Submit(Guid careEncounterId, Guid patientId, Guid providerId,
        int score, string? comment, DateTimeOffset now)
    {
        if (careEncounterId == Guid.Empty) throw new DomainValidationException("معرّف الجلسة مطلوب.");
        if (patientId == Guid.Empty) throw new DomainValidationException("معرّف المريض مطلوب.");
        if (providerId == Guid.Empty) throw new DomainValidationException("معرّف مقدم الرعاية مطلوب.");
        if (score < 1 || score > 5) throw new DomainValidationException("الدرجة يجب أن تكون بين 1 و5.");

        return new ServiceRating
        {
            Id = Guid.NewGuid(),
            CareEncounterId = careEncounterId,
            PatientId = patientId,
            ProviderId = providerId,
            Score = score,
            Comment = comment?.Trim(),
            SubmittedAt = now
        };
    }
}
