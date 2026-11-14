using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class LoginHistory : Entity
{
    public const int MaxIpAddressLength = 64;
    public const int MaxUserAgentLength = 512;
    public const int MaxFailureReasonLength = 160;

    private LoginHistory() { }
    public Guid? UserAccountId { get; private set; }
    public string LoginIdentifier { get; private set; } = string.Empty;
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }

    public static LoginHistory Record(Guid? accountId, string identifier, bool success, string? reason, string? ip, string? agent, DateTimeOffset at) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserAccountId = accountId,
            LoginIdentifier = identifier,
            IsSuccessful = success,
            FailureReason = reason.TruncateTo(MaxFailureReasonLength),
            IpAddress = ip.TruncateTo(MaxIpAddressLength),
            UserAgent = agent.TruncateTo(MaxUserAgentLength),
            AttemptedAt = at
        };
}
