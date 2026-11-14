using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.ProfessionalApproval;
using ApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class ProfessionalApprovalConfiguration : IEntityTypeConfiguration<ApprovalEntity>
{
    public void Configure(EntityTypeBuilder<ApprovalEntity> b)
    {
        b.ToTable("ProfessionalApprovals");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProfessionalType).HasConversion<int>().IsRequired();
        b.Property(x => x.ApprovalStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.SubmittedAt).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.DecidedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.DecisionReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ProviderId, x.ProfessionalType });
        b.HasMany(x => x.Reviews)
            .WithOne()
            .HasForeignKey(x => x.ApprovalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProfessionalApprovalReviewConfiguration : IEntityTypeConfiguration<ProfessionalApprovalReview>
{
    public void Configure(EntityTypeBuilder<ProfessionalApprovalReview> b)
    {
        b.ToTable("ProfessionalApprovalReviews");
        b.HasKey(x => x.Id);
        b.Property(x => x.ReviewAction).HasConversion<int>().IsRequired();
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.ReviewedAt).HasColumnType("datetimeoffset").IsRequired();
    }
}
