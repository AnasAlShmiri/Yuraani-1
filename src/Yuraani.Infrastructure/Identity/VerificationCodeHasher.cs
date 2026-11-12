using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Yuraani.Application.Features.Identity.Auth.Abstractions;

namespace Yuraani.Infrastructure.Identity;

public sealed class VerificationCodeHasher : IVerificationCodeHasher
{
    private readonly byte[] _key;

    public VerificationCodeHasher(IConfiguration configuration)
    {
        var secret = configuration["Verification:HashKey"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("Verification:HashKey must be configured with at least 32 bytes.");

        _key = Encoding.UTF8.GetBytes(secret);
    }

    // يستخدم مفتاحاً سرياً لأن رموز التحقق قصيرة ويمكن تخمينها بقوة التجربة.
    public string Hash(string code)
    {
        using var hmac = new HMACSHA256(_key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
    }
}
