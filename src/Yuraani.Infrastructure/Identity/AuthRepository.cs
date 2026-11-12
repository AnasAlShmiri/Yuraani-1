using Microsoft.EntityFrameworkCore;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Domain.Identity;
using Yuraani.Infrastructure.Persistence;
namespace Yuraani.Infrastructure.Identity;
public sealed class AuthRepository(YuraaniDbContext db):IAuthRepository
{
 public Task<bool> PhoneExistsAsync(string phone,CancellationToken ct)=>db.UserAccounts.AnyAsync(x=>x.NormalizedPhoneNumber==phone,ct);
 public Task<bool> EmailExistsAsync(string email,CancellationToken ct)=>db.UserAccounts.AnyAsync(x=>x.NormalizedEmail==email,ct);
 public async Task AddAccountAsync(UserAccount account,CancellationToken ct)=>await db.UserAccounts.AddAsync(account,ct);
 public async Task AddChallengeAsync(VerificationChallenge challenge,CancellationToken ct)=>await db.VerificationChallenges.AddAsync(challenge,ct);
 public async Task AddLoginHistoryAsync(LoginHistory history,CancellationToken ct)=>await db.LoginHistory.AddAsync(history,ct);
 public Task<UserAccount?> FindByIdentifierAsync(string id,bool email,CancellationToken ct)=>db.UserAccounts.SingleOrDefaultAsync(x=>email?x.NormalizedEmail==id:x.NormalizedPhoneNumber==id,ct);
 public Task<UserAccount?> FindByIdAsync(Guid id,CancellationToken ct)=>db.UserAccounts.SingleOrDefaultAsync(x=>x.Id==id,ct);
 public Task<VerificationChallenge?> FindLatestChallengeAsync(Guid id,VerificationChannel channel,CancellationToken ct)=>db.VerificationChallenges.Where(x=>x.UserAccountId==id&&x.Channel==channel&&x.ConsumedAt==null).OrderByDescending(x=>x.CreatedAt).FirstOrDefaultAsync(ct);
 public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}