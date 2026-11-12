using Xunit;
using Yuraani.Domain.Identity;

namespace Yuraani.UnitTests.Identity;

public sealed class VerificationChallengeTests
{
    [Fact]
    public void Create_ShouldBeUsableForTenMinutes()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = VerificationChallenge.Create(
            Guid.NewGuid(), VerificationChannel.Phone, "A1B2", now);

        Assert.True(challenge.IsUsable(now));
        Assert.True(challenge.IsUsable(now.AddMinutes(9)));
        Assert.False(challenge.IsUsable(now.AddMinutes(10)));
    }

    [Fact]
    public void RecordFailedAttempt_ShouldRevokeChallengeAfterFiveAttempts()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = VerificationChallenge.Create(
            Guid.NewGuid(), VerificationChannel.Email, "A1B2", now);

        for (var attempt = 0; attempt < VerificationChallenge.MaximumAttempts; attempt++)
            challenge.RecordFailedAttempt(now.AddSeconds(attempt));

        Assert.False(challenge.IsUsable(now.AddSeconds(5)));
        Assert.Throws<InvalidOperationException>(
            () => challenge.RecordFailedAttempt(now.AddSeconds(6)));
    }

    [Fact]
    public void Consume_ShouldMakeChallengeUnusable()
    {
        var now = DateTimeOffset.UtcNow;
        var challenge = VerificationChallenge.Create(
            Guid.NewGuid(), VerificationChannel.Phone, "A1B2", now);

        challenge.Consume(now);

        Assert.False(challenge.IsUsable(now));
    }

    [Fact]
    public void Matches_ShouldCompareHashesWithoutThrowingOnInvalidHex()
    {
        var challenge = VerificationChallenge.Create(
            Guid.NewGuid(), VerificationChannel.Phone, "A1B2", DateTimeOffset.UtcNow);

        Assert.True(challenge.Matches("A1B2"));
        Assert.False(challenge.Matches("A1B3"));
        Assert.False(challenge.Matches("not-hex"));
    }
}
