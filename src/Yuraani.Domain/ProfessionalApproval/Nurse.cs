using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>بيانات التمريض وتراخيصه المرتبطة بمقدم الرعاية.</summary>
public sealed class Nurse : Entity
{
    public Guid CareProviderId { get; private set; }
    public string NursingLicenseNumber { get; private set; } = string.Empty;
    public string Qualification { get; private set; } = string.Empty;
    public string NursingLevel { get; private set; } = string.Empty;

    private Nurse() { }

    /// <summary>يُنشئ سجل ممرض جديد.</summary>
    public static Nurse Create(Guid careProviderId, string nursingLicenseNumber, string qualification, string nursingLevel)
    {
        if (careProviderId == Guid.Empty)
            throw new DomainValidationException("معرّف مقدم الرعاية مطلوب.");
        if (string.IsNullOrWhiteSpace(nursingLicenseNumber))
            throw new DomainValidationException("رقم ترخيص التمريض مطلوب.");
        if (string.IsNullOrWhiteSpace(qualification))
            throw new DomainValidationException("المؤهل التمريضي مطلوب.");

        return new Nurse
        {
            Id = Guid.NewGuid(),
            CareProviderId = careProviderId,
            NursingLicenseNumber = nursingLicenseNumber.Trim(),
            Qualification = qualification.Trim(),
            NursingLevel = nursingLevel?.Trim() ?? string.Empty
        };
    }
}
