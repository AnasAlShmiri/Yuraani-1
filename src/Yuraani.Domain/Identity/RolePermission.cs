namespace Yuraani.Domain.Identity;
public sealed class RolePermission
{
    private RolePermission() { }
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public static RolePermission Grant(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty || permissionId == Guid.Empty) throw new ArgumentException("Role and permission are required.");
        return new RolePermission { RoleId = roleId, PermissionId = permissionId };
    }
}