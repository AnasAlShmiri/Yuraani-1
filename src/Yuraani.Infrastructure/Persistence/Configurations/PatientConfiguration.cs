using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.Patients;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.ToTable("Patients");
        b.HasKey(x => x.Id);
        b.Property(x => x.AccountId).IsRequired();
        b.Property(x => x.Gender).HasConversion<int>().IsRequired();
        b.Property(x => x.Address).HasMaxLength(500).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasIndex(x => x.AccountId).IsUnique();
    }
}

public sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> b)
    {
        b.ToTable("MedicalRecords");
        b.HasKey(x => x.Id);
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.BloodGroup).HasConversion<int>().IsRequired();
        b.Property(x => x.ChronicDiseases).HasColumnType("nvarchar(max)");
        b.Property(x => x.Allergies).HasColumnType("nvarchar(max)");
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.LastUpdatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasMany(x => x.LabTestResults)
            .WithOne()
            .HasForeignKey(x => x.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.PatientId).IsUnique();
    }
}

public sealed class LabTestResultConfiguration : IEntityTypeConfiguration<LabTestResult>
{
    public void Configure(EntityTypeBuilder<LabTestResult> b)
    {
        b.ToTable("LabTestResults");
        b.HasKey(x => x.Id);
        b.Property(x => x.TestName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Instructions).HasMaxLength(1000);
        b.Property(x => x.ResultFileUrl).HasMaxLength(500);
        b.Property(x => x.PatientNotes).HasMaxLength(1000);
        b.Property(x => x.DoctorReviewNotes).HasMaxLength(1000);
        b.Property(x => x.RequestedAt).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.UploadedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.ReviewedAt).HasColumnType("datetimeoffset");
    }
}
