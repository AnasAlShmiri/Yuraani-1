using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Yuraani.Infrastructure.Persistence;

namespace Yuraani.API.Security;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(YuraaniDbContext db) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var subject = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(subject, out var accountId)) return;

        var hasPermission = await (
            from userRole in db.UserRoles
            join rolePermission in db.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserAccountId == accountId && permission.Code == requirement.Code
            select permission.Id).AnyAsync();

        if (hasPermission) context.Succeed(requirement);
    }
}
