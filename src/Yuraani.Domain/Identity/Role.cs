using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class Role : Entity
{
    private Role() { }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 80) throw new DomainValidationException("Role name is required and must not exceed 80 characters.");
        return new Role { Id = Guid.NewGuid(), Name = name.Trim(), Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim() };
    }
}
