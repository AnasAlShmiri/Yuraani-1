using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class Role : Entity<Guid>
{
    private Role() { }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 80) throw new ArgumentException("Role name is required and must not exceed 80 characters.", nameof(name));
        return new Role { Id = Guid.NewGuid(), Name = name.Trim(), Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim() };
    }
}