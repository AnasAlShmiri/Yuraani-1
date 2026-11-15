using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>بيانات الطبيب وتراخيصه المرتبطة بمقدم الرعاية.</summary>
public sealed class Doctor : Entity
{
    public Guid CareProviderId { get; private set; }
    public string MedicalLicenseNumber { get; private set; } = string.Empty;
    public string ProfessionalTitle { get; private set; } = string.Empty;
    public string SpecialtyName { get; private set; } = string.Empty;

    private Doctor() { }

    /// <summary>يُنشئ سجل طبيب جديد.</summary>
    public static Doctor Create(Guid careProviderId, string medicalLicenseNumber, string professionalTitle, string specialtyName)
    {
        if (careProviderId == Guid.Empty)
            throw new DomainValidationException("معرّف مقدم الرعاية مطلوب.");
        if (string.IsNullOrWhiteSpace(medicalLicenseNumber))
            throw new DomainValidationException("رقم الترخيص الطبي مطلوب.");
        if (string.IsNullOrWhiteSpace(specialtyName))
            throw new DomainValidationException("التخصص الطبي مطلوب.");

        return new Doctor
        {
            Id = Guid.NewGuid(),
            CareProviderId = careProviderId,
            MedicalLicenseNumber = medicalLicenseNumber.Trim(),
            ProfessionalTitle = professionalTitle?.Trim() ?? string.Empty,
            SpecialtyName = specialtyName.Trim()
        };
    }
}
