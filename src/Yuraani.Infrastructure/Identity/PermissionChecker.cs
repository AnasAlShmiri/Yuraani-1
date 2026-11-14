using Microsoft.EntityFrameworkCore;
using Yuraani.Application.Common.Abstractions;
using Yuraani.Infrastructure.Persistence;

namespace Yuraani.Infrastructure.Identity;

/// <summary>تنفيذ التحقق من الصلاحيات عبر قاعدة البيانات.</summary>
public sealed class PermissionChecker(YuraaniDbContext db) : IPermissionChecker
{
    public Task<bool> HasPermissionAsync(Guid accountId, string permissionCode, CancellationToken ct = default) =>
        (from userRole in db.UserRoles
         join rolePermission in db.RolePermissions on userRole.RoleId equals rolePermission.RoleId
         join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
         where userRole.UserAccountId == accountId && permission.Code == permissionCode
         select permission.Id).AnyAsync(ct);
}
