using Microsoft.AspNetCore.Authorization;
using Yuraani.Application.Common.Abstractions;

namespace Yuraani.API.Security;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(IPermissionChecker permissionChecker)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var subject = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(subject, out var accountId)) return;

        var hasPermission = await permissionChecker.HasPermissionAsync(accountId, requirement.Code);
        if (hasPermission) context.Succeed(requirement);
    }
}
