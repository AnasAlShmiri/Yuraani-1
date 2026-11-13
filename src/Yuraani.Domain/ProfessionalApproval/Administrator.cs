using Yuraani.Domain.Common;

namespace Yuraani.Domain.ProfessionalApproval;

/// <summary>ملف المسؤول الإداري المرتبط بحساب الدخول.</summary>
public sealed class Administrator : Entity<Guid>
{
    private Administrator() { }

    public Guid UserAccountId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Administrator Create(Guid userAccountId, DateTimeOffset now)
    {
        if (userAccountId == Guid.Empty) throw new ArgumentException("User account is required.", nameof(userAccountId));
        return new Administrator { Id = Guid.NewGuid(), UserAccountId = userAccountId, CreatedAt = now };
    }
}
