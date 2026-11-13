using Xunit;
using Yuraani.Domain.Identity;

namespace Yuraani.UnitTests.Identity;

public sealed class RefreshSessionTests
{
    [Fact]
    public void Session_ShouldBeActiveUntilRevokedOrExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var session = RefreshSession.Create(Guid.NewGuid(), new string('A', 64), now, null, null);

        Assert.True(session.IsActive(now.AddDays(1)));
        Assert.False(session.IsActive(now.AddDays(31)));
    }

    [Fact]
    public void RevokedSession_ShouldNoLongerBeActive()
    {
        var now = DateTimeOffset.UtcNow;
        var session = RefreshSession.Create(Guid.NewGuid(), new string('B', 64), now, null, null);
        session.Revoke(now.AddMinutes(1));

        Assert.False(session.IsActive(now.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(() => session.Revoke(now.AddMinutes(3)));
    }

    [Fact]
    public void RoleAssignment_ShouldRejectSelfAssignment()
    {
        var userId = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => UserRole.Assign(userId, Guid.NewGuid(), DateTimeOffset.UtcNow, userId));
    }
}