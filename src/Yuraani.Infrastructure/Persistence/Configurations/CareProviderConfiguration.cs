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
        b.Property(x => x.AccountId).IsRequired();
        b.Property(x => x.ProviderType).HasConversion<int>().IsRequired();
        b.Property(x => x.ApprovalStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.ServiceArea).HasMaxLength(160).IsRequired();
        b.Property(x => x.BasePrice).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.AverageRating).HasColumnType("decimal(3,2)").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasIndex(x => x.AccountId).IsUnique();
    }
}

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> b)
    {
        b.ToTable("Doctors");
        b.HasKey(x => x.Id);
        b.Property(x => x.MedicalLicenseNumber).HasMaxLength(80).IsRequired();
        b.Property(x => x.ProfessionalTitle).HasMaxLength(160);
        b.Property(x => x.SpecialtyName).HasMaxLength(200).IsRequired();
        b.HasOne<CareProvider>().WithMany().HasForeignKey(x => x.CareProviderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NurseConfiguration : IEntityTypeConfiguration<Nurse>
{
    public void Configure(EntityTypeBuilder<Nurse> b)
    {
        b.ToTable("Nurses");
        b.HasKey(x => x.Id);
        b.Property(x => x.NursingLicenseNumber).HasMaxLength(80).IsRequired();
        b.Property(x => x.Qualification).HasMaxLength(200).IsRequired();
        b.Property(x => x.NursingLevel).HasMaxLength(100);
        b.HasOne<CareProvider>().WithMany().HasForeignKey(x => x.CareProviderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> b)
    {
        b.ToTable("Administrators");
        b.HasKey(x => x.Id);
        b.Property(x => x.AccountId).IsRequired();
        b.HasIndex(x => x.AccountId).IsUnique();
    }
}

public sealed class ProfessionalLicenseConfiguration : IEntityTypeConfiguration<ProfessionalLicense>
{
    public void Configure(EntityTypeBuilder<ProfessionalLicense> b)
    {
        b.ToTable("ProfessionalLicenses");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProfessionalType).HasConversion<int>().IsRequired();
        b.Property(x => x.LicenseNumber).HasMaxLength(80).IsRequired();
        b.Property(x => x.DocumentPath).HasMaxLength(500).IsRequired();
        b.Property(x => x.ReviewStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.ReviewedAt).HasColumnType("datetimeoffset");
    }
}
