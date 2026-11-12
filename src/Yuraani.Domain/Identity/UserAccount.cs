using Yuraani.Domain.Common;
namespace Yuraani.Domain.Identity;
public sealed class UserAccount : Entity<Guid>
{
 private UserAccount() { }
 public string FullName { get; private set; } = string.Empty;
 public string PhoneNumber { get; private set; } = string.Empty;
 public string NormalizedPhoneNumber { get; private set; } = string.Empty;
 public string Email { get; private set; } = string.Empty;
 public string NormalizedEmail { get; private set; } = string.Empty;
 public string PasswordHash { get; private set; } = string.Empty;
 public AccountStatus AccountStatus { get; private set; }
 public bool IsPhoneVerified { get; private set; }
 public bool IsEmailVerified { get; private set; }
 public int ConsecutiveFailedLoginAttempts { get; private set; }
 public DateTimeOffset? LockoutEnd { get; private set; }
 public DateTimeOffset CreatedAt { get; private set; }
 public DateTimeOffset? UpdatedAt { get; private set; }
 public static UserAccount Register(string fullName,string phone,string email,string passwordHash,DateTimeOffset now)
 {
  if(string.IsNullOrWhiteSpace(fullName)||string.IsNullOrWhiteSpace(phone)||string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Required account data is missing.");
  return new UserAccount { Id=Guid.NewGuid(), FullName=fullName.Trim(), PhoneNumber=phone.Trim(), NormalizedPhoneNumber=NormalizePhone(phone), Email=email.Trim(), NormalizedEmail=email.Trim().ToUpperInvariant(), PasswordHash=passwordHash, AccountStatus=AccountStatus.PendingVerification, CreatedAt=now };
 }
 public void MarkVerified(VerificationChannel channel,DateTimeOffset now)
 {
  if(channel==VerificationChannel.Phone) IsPhoneVerified=true; else IsEmailVerified=true;
  if(IsPhoneVerified&&IsEmailVerified&&AccountStatus==AccountStatus.PendingVerification) AccountStatus=AccountStatus.Active;
  UpdatedAt=now;
 }
 public bool IsLockedOut(DateTimeOffset now)=>LockoutEnd.HasValue&&LockoutEnd.Value>now;
 public void RecordFailedLogin(DateTimeOffset now){ConsecutiveFailedLoginAttempts++;if(ConsecutiveFailedLoginAttempts>=5){LockoutEnd=now.AddMinutes(15);ConsecutiveFailedLoginAttempts=0;}UpdatedAt=now;}
 public void RecordSuccessfulLogin(DateTimeOffset now){ConsecutiveFailedLoginAttempts=0;LockoutEnd=null;UpdatedAt=now;}
 public static string NormalizePhone(string value)=>new(value.Where(char.IsDigit).ToArray());
}