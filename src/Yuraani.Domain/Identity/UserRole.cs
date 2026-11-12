namespace Yuraani.Domain.Identity;
public sealed class UserRole
{
    private UserRole() { }
    public Guid UserAccountId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public Guid? AssignedByUserAccountId { get; private set; }
    public static UserRole Assign(Guid userId, Guid roleId, DateTimeOffset at, Guid? assignedBy = null)
    {
        if (userId == Guid.Empty || roleId == Guid.Empty) throw new ArgumentException("User and role are required.");
        if (userId == assignedBy) throw new InvalidOperationException("Users cannot assign roles to themselves.");
        return new UserRole { UserAccountId = userId, RoleId = roleId, AssignedAt = at, AssignedByUserAccountId = assignedBy };
    }
}