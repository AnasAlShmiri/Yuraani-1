using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>ملف المسؤول الإداري المرتبط بحساب الدخول.</summary>
public sealed class Administrator : Entity
{
    public Guid AccountId { get; private set; }
    public string PermissionLevel { get; private set; } = "Standard";

    private Administrator() { }

    public static Administrator Create(Guid accountId, string permissionLevel = "Standard")
    {
        if (accountId == Guid.Empty)
            throw new DomainValidationException("معرّف الحساب مطلوب.");
        if (permissionLevel?.Trim().Length > 50)
            throw new DomainValidationException("مستوى الصلاحية لا يتجاوز 50 حرفاً.");

        return new Administrator
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            PermissionLevel = string.IsNullOrWhiteSpace(permissionLevel) ? "Standard" : permissionLevel.Trim()
        };
    }
}
