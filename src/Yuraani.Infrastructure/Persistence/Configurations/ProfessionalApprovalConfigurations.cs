using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.ProfessionalApproval;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class CareProviderConfiguration : IEntityTypeConfiguration<CareProvider>
{
    public void Configure(EntityTypeBuilder<CareProvider> b)
    {
        b.ToTable("CareProviders");
        b.HasKey(x => x.Id);
        b.Property(x => x.BasePrice).HasPrecision(12, 2);
        b.Property(x => x.ServiceArea).HasColumnType("nvarchar(max)");
        b.HasIndex(x => x.UserAccountId).IsUnique();
        b.HasOne<Yuraani.Domain.Identity.UserAccount>().WithMany().HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> b)
    {
        b.ToTable("Administrators");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserAccountId).IsUnique();
        b.HasOne<Yuraani.Domain.Identity.UserAccount>().WithMany().HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProfessionalApprovalConfiguration : IEntityTypeConfiguration<ProfessionalApproval>
{
    public void Configure(EntityTypeBuilder<ProfessionalApproval> b)
    {
        b.ToTable("ProfessionalApprovals", t =>
            t.HasCheckConstraint("CK_ProfessionalApprovals_AttemptNumber", "[AttemptNumber] BETWEEN 1 AND 3"));
        b.HasKey(x => x.Id);
        b.Property(x => x.ProfessionalType).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.ApprovalStatus).HasConversion<string>().HasMaxLength(25).IsRequired();
        b.Property(x => x.DecisionReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ProviderId, x.ProfessionalType, x.AttemptNumber }).IsUnique();
        b.HasOne<CareProvider>().WithMany().HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Administrator>().WithMany().HasForeignKey(x => x.ReviewedByAdminId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProfessionalApprovalReviewConfiguration : IEntityTypeConfiguration<ProfessionalApprovalReview>
{
    public void Configure(EntityTypeBuilder<ProfessionalApprovalReview> b)
    {
        b.ToTable("ProfessionalApprovalReviews");
        b.HasKey(x => x.Id);
        b.Property(x => x.ReviewAction).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.HasOne<ProfessionalApproval>().WithMany().HasForeignKey(x => x.ApprovalId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Administrator>().WithMany().HasForeignKey(x => x.AdminId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ApprovalId, x.CreatedAt });
    }
}

public sealed class ProfessionalLicenseConfiguration : IEntityTypeConfiguration<ProfessionalLicense>
{
    public void Configure(EntityTypeBuilder<ProfessionalLicense> b)
    {
        b.ToTable("ProfessionalLicenses");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProfessionalType).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.LicenseNumber).HasMaxLength(80).IsRequired();
        b.Property(x => x.DocumentPath).HasMaxLength(500).IsRequired();
        b.Property(x => x.ReviewStatus).HasConversion<string>().HasMaxLength(25).IsRequired();
        b.HasOne<CareProvider>().WithMany().HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ProviderId, x.ProfessionalType, x.LicenseNumber }).IsUnique();
        b.HasIndex(x => new { x.ProviderId, x.ProfessionalType }).IsUnique().HasFilter("[IsCurrent] = 1");
    }
}
