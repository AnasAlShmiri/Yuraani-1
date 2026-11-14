using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>بيانات الاستشارة عبر الفيديو.</summary>
public sealed class VideoConsultation : Entity
{
    private VideoConsultation() { }

    /// <summary>معرّف جلسة الرعاية.</summary>
    public Guid CareEncounterId { get; private set; }

    /// <summary>مرجع الجلسة.</summary>
    public string SessionReference { get; private set; } = string.Empty;

    /// <summary>وقت بدء الجلسة.</summary>
    public DateTimeOffset? StartedAt { get; private set; }

    /// <summary>وقت انتهاء الجلسة.</summary>
    public DateTimeOffset? EndedAt { get; private set; }

    /// <summary>حالة الجلسة.</summary>
    public SessionStatus SessionStatus { get; private set; }

    /// <summary>يُنشئ جلسة فيديو.</summary>
    public static VideoConsultation Create(Guid careEncounterId, string sessionReference)
    {
        if (careEncounterId == Guid.Empty) throw new DomainValidationException("معرّف الجلسة مطلوب.");
        if (string.IsNullOrWhiteSpace(sessionReference))
            throw new DomainValidationException("مرجع الجلسة مطلوب.");
        return new VideoConsultation
        {
            Id = Guid.NewGuid(),
            CareEncounterId = careEncounterId,
            SessionReference = sessionReference.Trim(),
            SessionStatus = SessionStatus.Waiting
        };
    }

    /// <summary>يبدأ الجلسة.</summary>
    public void StartSession(DateTimeOffset now)
    {
        if (SessionStatus != SessionStatus.Waiting) throw new BusinessRuleException("الجلسة ليست في حالة انتظار.");
        SessionStatus = SessionStatus.Live;
        StartedAt = now;
    }

    /// <summary>ينهي الجلسة.</summary>
    public void EndSession(DateTimeOffset now)
    {
        if (SessionStatus != SessionStatus.Live) throw new BusinessRuleException("الجلسة غير نشطة.");
        SessionStatus = SessionStatus.Ended;
        EndedAt = now;
    }
}
