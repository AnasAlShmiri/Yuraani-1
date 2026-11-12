using Xunit;
using Yuraani.Domain.Identity;

namespace Yuraani.UnitTests.Identity;

public sealed class UserAccountTests
{
    [Theory]
    [InlineData("+967771234567", "+967771234567")]
    [InlineData("00967771234567", "+967771234567")]
    public void NormalizePhone_ShouldReturnInternationalFormat(string input, string expected)
    {
        Assert.Equal(expected, UserAccount.NormalizePhone(input));
    }

    [Theory]
    [InlineData("+967771234567", true)]
    [InlineData("771234567", false)]
    [InlineData("+123", false)]
    public void IsValidInternationalPhone_ShouldRequireE164(string input, bool expected)
    {
        Assert.Equal(expected, UserAccount.IsValidInternationalPhone(input));
    }

    [Fact]
    public void Register_ShouldRequireInternationalPhone()
    {
        Assert.Throws<ArgumentException>(() =>
            UserAccount.Register("Test User", "771234567", "test@example.com", "password-hash", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Account_ShouldBecomeActiveOnlyAfterBothContactsAreVerified()
    {
        var now = DateTimeOffset.UtcNow;
        var account = UserAccount.Register(
            "Test User", "+967771234567", "test@example.com", "password-hash", now);

        account.MarkVerified(VerificationChannel.Phone, now.AddMinutes(1));
        Assert.Equal(AccountStatus.PendingVerification, account.AccountStatus);

        account.MarkVerified(VerificationChannel.Email, now.AddMinutes(2));
        Assert.Equal(AccountStatus.Active, account.AccountStatus);
    }
}
