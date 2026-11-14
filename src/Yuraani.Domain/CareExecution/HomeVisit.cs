using Yuraani.Domain.Common;
namespace Yuraani.Domain.CareExecution;

/// <summary>بيانات الزيارة المنزلية.</summary>
public sealed class HomeVisit : Entity
{
    private HomeVisit() { }

    /// <summary>معرّف جلسة الرعاية.</summary>
    public Guid CareEncounterId { get; private set; }

    /// <summary>عنوان الزيارة.</summary>
    public string VisitAddress { get; private set; } = string.Empty;

    /// <summary>وقت الوصول.</summary>
    public DateTimeOffset? ArrivalTime { get; private set; }

    /// <summary>وقت المغادرة.</summary>
    public DateTimeOffset? DepartureTime { get; private set; }

    /// <summary>يُنشئ سجل زيارة منزلية.</summary>
    public static HomeVisit Create(Guid careEncounterId, string visitAddress)
    {
        if (careEncounterId == Guid.Empty) throw new DomainValidationException("معرّف الجلسة مطلوب.");
        if (string.IsNullOrWhiteSpace(visitAddress) || visitAddress.Trim().Length > 500)
            throw new DomainValidationException("عنوان الزيارة مطلوب ولا يتجاوز 500 حرف.");
        return new HomeVisit { Id = Guid.NewGuid(), CareEncounterId = careEncounterId, VisitAddress = visitAddress.Trim() };
    }

    /// <summary>يثبّت وقت الوصول.</summary>
    public void ConfirmArrival(DateTimeOffset now)
    {
        if (ArrivalTime.HasValue) throw new BusinessRuleException("تم تسجيل وقت الوصول مسبقاً.");
        ArrivalTime = now;
    }

    /// <summary>يثبّت وقت المغادرة.</summary>
    public void FinishVisit(DateTimeOffset now)
    {
        if (!ArrivalTime.HasValue) throw new BusinessRuleException("لا يمكن إنهاء الزيارة قبل تسجيل الوصول.");
        if (DepartureTime.HasValue) throw new BusinessRuleException("تم تسجيل وقت المغادرة مسبقاً.");
        DepartureTime = now;
    }
}
