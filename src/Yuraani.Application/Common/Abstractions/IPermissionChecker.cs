namespace Yuraani.Application.Common.Abstractions;

/// <summary>واجهة التحقق من صلاحيات المستخدم.</summary>
public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(Guid accountId, string permissionCode, CancellationToken ct = default);
}
