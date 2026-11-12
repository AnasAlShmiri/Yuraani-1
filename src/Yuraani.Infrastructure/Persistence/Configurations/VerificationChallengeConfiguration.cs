using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.Identity;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class VerificationChallengeConfiguration : IEntityTypeConfiguration<VerificationChallenge>
{
    public void Configure(EntityTypeBuilder<VerificationChallenge> builder)
    {
        builder.ToTable("VerificationChallenges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Channel).HasConversion<int>().IsRequired();
        builder.Property(x => x.CodeHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.RevokedAt);
        builder.HasIndex(x => new { x.UserAccountId, x.Channel, x.CreatedAt });
        builder.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Cascade);
    }
}
