using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.Identity;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> b)
    {
        b.ToTable("LoginHistory");
        b.HasKey(x => x.Id);
        b.Property(x => x.LoginIdentifier).HasMaxLength(254).IsRequired();
        b.Property(x => x.FailureReason).HasMaxLength(160);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.Property(x => x.UserAgent).HasMaxLength(512);
        b.Property(x => x.AttemptedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasIndex(x => x.AttemptedAt);
        b.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.SetNull);
    }
}
