using Yuraani.Domain.Common;

namespace Yuraani.Domain.Patients;

public enum Gender { Male = 1, Female = 2 }

public enum BloodGroup
{
    Unknown = 0,
    APositive = 1, ANegative = 2,
    BPositive = 3, BNegative = 4,
    ABPositive = 5, ABNegative = 6,
    OPositive = 7, ONegative = 8
}

public sealed class Patient : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Patient() { }

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

    public void UpdateAddress(string address, double? latitude, double? longitude)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
            throw new DomainValidationException("العنوان غير صالح.");
        Address = address.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }
}
