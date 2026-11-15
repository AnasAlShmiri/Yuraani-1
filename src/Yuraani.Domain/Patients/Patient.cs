using Yuraani.Domain.Common;

namespace Yuraani.Domain.Patients;

/// <summary>مريض مسجّل في المنصة.</summary>
public sealed class Patient : AggregateRoot
{
    /// <summary>معرّف حساب المستخدم.</summary>
    public Guid AccountId { get; private set; }

    /// <summary>تاريخ الميلاد.</summary>
    public DateOnly DateOfBirth { get; private set; }

    /// <summary>الجنس.</summary>
    public Gender Gender { get; private set; }

    /// <summary>العنوان النصي.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>خط العرض.</summary>
    public double? Latitude { get; private set; }

    /// <summary>خط الطول.</summary>
    public double? Longitude { get; private set; }

    /// <summary>تاريخ الإنشاء.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    private Patient() { }

    /// <summary>يُنشئ سجل مريض جديد.</summary>
    public static Patient Create(
        Guid accountId,
        DateOnly dateOfBirth,
        Gender gender,
        string address,
        double? latitude = null,
        double? longitude = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainValidationException("معرّف الحساب مطلوب.");
        if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
            throw new DomainValidationException("عنوان المريض مطلوب ولا يتجاوز 300 حرف.");

        return new Patient
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Address = address.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>يحدّث بيانات العنوان والموقع.</summary>
    public void UpdateAddress(string address, double? latitude, double? longitude)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
            throw new DomainValidationException("العنوان غير صالح.");
        Address = address.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }
}
