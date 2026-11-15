using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>مقدم الرعاية الصحية (طبيب أو ممرض) المسجّل في المنصة.</summary>
public class CareProvider : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public ProfessionalType ProviderType { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public bool IsAvailable { get; private set; }
    public string ServiceArea { get; private set; } = string.Empty;
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public decimal BasePrice { get; private set; }
    public int YearsOfExperience { get; private set; }
    public decimal AverageRating { get; private set; }
    public int RatingsCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CareProvider() { }

    /// <summary>يُنشئ مقدم رعاية جديد.</summary>
    public static CareProvider Create(
        Guid accountId,
        ProfessionalType providerType,
        string serviceArea,
        decimal basePrice,
        int yearsOfExperience,
        double? latitude = null,
        double? longitude = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainValidationException("معرّف الحساب مطلوب.");
        if (!Enum.IsDefined(providerType))
            throw new DomainValidationException("نوع مقدم الرعاية غير صالح.");
        ValidateProfile(serviceArea, basePrice, yearsOfExperience, latitude, longitude);

        return new CareProvider
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            ProviderType = providerType,
            ApprovalStatus = ApprovalStatus.Pending,
            IsAvailable = false,
            ServiceArea = serviceArea.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            BasePrice = basePrice,
            YearsOfExperience = yearsOfExperience,
            AverageRating = 0m,
            RatingsCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>يحدّث بيانات الملف الشخصي.</summary>
    public void UpdateProfile(string serviceArea, decimal basePrice, int yearsOfExperience, double? latitude, double? longitude)
    {
        ValidateProfile(serviceArea, basePrice, yearsOfExperience, latitude, longitude);

        ServiceArea = serviceArea.Trim();
        BasePrice = basePrice;
        YearsOfExperience = yearsOfExperience;
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>يُعلّم الاعتماد المهني.</summary>
    public void MarkApproved() => ApprovalStatus = ApprovalStatus.Approved;

    /// <summary>يُرفض الاعتماد المهني.</summary>
    public void MarkRejected()
    {
        ApprovalStatus = ApprovalStatus.Rejected;
        IsAvailable = false;
    }

    /// <summary>يحدّث حالة التوافر (مع منع التفعيل قبل الاعتماد).</summary>
    public void UpdateAvailability(bool isAvailable)
    {
        if (isAvailable && ApprovalStatus != ApprovalStatus.Approved)
            throw new BusinessRuleException("لا يمكن تفعيل التوافر قبل اعتماد الحساب مهنياً.");
        IsAvailable = isAvailable;
    }

    /// <summary>يطبّق تقييماً جديداً ويحدّث المتوسط.</summary>
    public void ApplyNewRating(int score)
    {
        if (score is < 1 or > 5)
            throw new DomainValidationException("درجة التقييم يجب أن تكون بين 1 و 5.");

        var total = (AverageRating * RatingsCount) + score;
        RatingsCount++;
        AverageRating = Math.Round(total / RatingsCount, 2);
    }

    private static void ValidateProfile(string serviceArea, decimal basePrice, int yearsOfExperience, double? latitude, double? longitude)
    {
        if (string.IsNullOrWhiteSpace(serviceArea) || serviceArea.Trim().Length > 160)
            throw new DomainValidationException("منطقة الخدمة مطلوبة ولا تتجاوز 160 حرفاً.");
        if (basePrice < 0)
            throw new DomainValidationException("السعر الأساسي لا يمكن أن يكون سالباً.");
        if (yearsOfExperience < 0 || yearsOfExperience > 70)
            throw new DomainValidationException("سنوات الخبرة غير صالحة.");
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            throw new DomainValidationException("إحداثيات الموقع غير صالحة.");
    }
}
