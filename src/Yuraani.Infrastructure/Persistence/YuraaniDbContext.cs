using Microsoft.EntityFrameworkCore;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Persistence;
public sealed class YuraaniDbContext(DbContextOptions<YuraaniDbContext> options):DbContext(options)
{
 public DbSet<UserAccount> UserAccounts=>Set<UserAccount>();
 public DbSet<VerificationChallenge> VerificationChallenges=>Set<VerificationChallenge>();
 public DbSet<LoginHistory> LoginHistory=>Set<LoginHistory>();
 protected override void OnModelCreating(ModelBuilder modelBuilder){base.OnModelCreating(modelBuilder);modelBuilder.ApplyConfigurationsFromAssembly(typeof(YuraaniDbContext).Assembly);}
}