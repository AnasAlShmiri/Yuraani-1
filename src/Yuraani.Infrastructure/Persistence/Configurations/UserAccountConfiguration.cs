using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Persistence.Configurations;
public sealed class UserAccountConfiguration:IEntityTypeConfiguration<UserAccount>
{
 public void Configure(EntityTypeBuilder<UserAccount> b)
 {
  b.ToTable("UserAccounts");b.HasKey(x=>x.Id);b.Property(x=>x.FullName).HasMaxLength(160).IsRequired();b.Property(x=>x.PhoneNumber).HasMaxLength(32).IsRequired();b.Property(x=>x.NormalizedPhoneNumber).HasMaxLength(32).IsRequired();b.Property(x=>x.Email).HasMaxLength(254).IsRequired();b.Property(x=>x.NormalizedEmail).HasMaxLength(254).IsRequired();b.Property(x=>x.PasswordHash).HasMaxLength(512).IsRequired();b.Property(x=>x.AccountStatus).HasConversion<int>().IsRequired();b.HasIndex(x=>x.NormalizedPhoneNumber).IsUnique();b.HasIndex(x=>x.NormalizedEmail).IsUnique();
 }
}