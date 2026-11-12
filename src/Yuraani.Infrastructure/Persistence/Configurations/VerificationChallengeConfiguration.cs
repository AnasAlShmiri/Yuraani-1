using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Persistence.Configurations;
public sealed class VerificationChallengeConfiguration:IEntityTypeConfiguration<VerificationChallenge>
{
 public void Configure(EntityTypeBuilder<VerificationChallenge> b)
 {
  b.ToTable("VerificationChallenges");b.HasKey(x=>x.Id);b.Property(x=>x.Channel).HasConversion<int>().IsRequired();b.Property(x=>x.CodeHash).HasMaxLength(64).IsRequired();b.HasIndex(x=>new{x.UserAccountId,x.Channel,x.CreatedAt});b.HasOne<UserAccount>().WithMany().HasForeignKey(x=>x.UserAccountId).OnDelete(DeleteBehavior.Cascade);
 }
}