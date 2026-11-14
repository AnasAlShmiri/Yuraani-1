using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class Permission : Entity
{
    private Permission() { }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public static Permission Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 120) throw new DomainValidationException("Permission code is required.");
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 250) throw new DomainValidationException("Permission description is required.");
        return new Permission { Id = Guid.NewGuid(), Code = code.Trim(), Description = description.Trim() };
    }
}
