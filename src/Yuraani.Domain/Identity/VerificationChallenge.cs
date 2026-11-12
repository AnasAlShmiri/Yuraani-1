using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class VerificationChallenge : Entity<Guid>
{
 private VerificationChallenge(){}
 public Guid UserAccountId{get;private set;}
 public VerificationChannel Channel{get;private set;}
 public string CodeHash{get;private set;}=string.Empty;
 public DateTimeOffset ExpiresAt{get;private set;}
 public DateTimeOffset CreatedAt{get;private set;}
 public DateTimeOffset? ConsumedAt{get;private set;}
 public static VerificationChallenge Create(Guid accountId,VerificationChannel channel,string hash,DateTimeOffset now)=>new(){Id=Guid.NewGuid(),UserAccountId=accountId,Channel=channel,CodeHash=hash,CreatedAt=now,ExpiresAt=now.AddMinutes(10)};
 public bool IsUsable(DateTimeOffset now)=>ConsumedAt is null&&ExpiresAt>now;
 public bool Matches(string hash)=>string.Equals(CodeHash,hash,StringComparison.Ordinal);
 public void Consume(DateTimeOffset now){if(ConsumedAt is not null)throw new InvalidOperationException("Challenge already used.");ConsumedAt=now;}
}