using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.CareExecution;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class CareEncounterConfiguration : IEntityTypeConfiguration<CareEncounter>
{
    public void Configure(EntityTypeBuilder<CareEncounter> b)
    {
        b.ToTable("CareEncounters");
        b.HasKey(x => x.Id);
        b.Property(x => x.AppointmentId).IsRequired();
        b.Property(x => x.MedicalRecordId).IsRequired();
        b.Property(x => x.ProviderId).IsRequired();
        b.Property(x => x.EncounterStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.StartedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.CompletedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.CompletionNotes).HasMaxLength(2000);
        b.HasOne(x => x.HomeVisit).WithOne().HasForeignKey<HomeVisit>(x => x.CareEncounterId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.VideoConsultation).WithOne().HasForeignKey<VideoConsultation>(x => x.CareEncounterId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.AppointmentId).IsUnique();
    }
}

public sealed class HomeVisitConfiguration : IEntityTypeConfiguration<HomeVisit>
{
    public void Configure(EntityTypeBuilder<HomeVisit> b)
    {
        b.ToTable("HomeVisits");
        b.HasKey(x => x.Id);
        b.Property(x => x.VisitAddress).HasMaxLength(500).IsRequired();
        b.Property(x => x.ArrivalTime).HasColumnType("datetimeoffset");
        b.Property(x => x.DepartureTime).HasColumnType("datetimeoffset");
    }
}

public sealed class VideoConsultationConfiguration : IEntityTypeConfiguration<VideoConsultation>
{
    public void Configure(EntityTypeBuilder<VideoConsultation> b)
    {
        b.ToTable("VideoConsultations");
        b.HasKey(x => x.Id);
        b.Property(x => x.SessionReference).HasMaxLength(200).IsRequired();
        b.Property(x => x.SessionStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.StartedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.EndedAt).HasColumnType("datetimeoffset");
    }
}

public sealed class TreatmentPlanConfiguration : IEntityTypeConfiguration<TreatmentPlan>
{
    public void Configure(EntityTypeBuilder<TreatmentPlan> b)
    {
        b.ToTable("TreatmentPlans");
        b.HasKey(x => x.Id);
        b.Property(x => x.MedicalRecordId).IsRequired();
        b.Property(x => x.CareEncounterId).IsRequired();
        b.Property(x => x.DoctorProviderId).IsRequired();
        b.Property(x => x.DiagnosisSummary).HasColumnType("nvarchar(max)").IsRequired();
        b.Property(x => x.GeneralInstructions).HasColumnType("nvarchar(max)");
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.LastUpdatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.TreatmentPlanId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.CareEncounterId);
    }
}

public sealed class TreatmentPlanItemConfiguration : IEntityTypeConfiguration<TreatmentPlanItem>
{
    public void Configure(EntityTypeBuilder<TreatmentPlanItem> b)
    {
        b.ToTable("TreatmentPlanItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.MedicationOrProcedureName).HasMaxLength(300).IsRequired();
        b.Property(x => x.Dosage).HasMaxLength(200);
        b.Property(x => x.UsageInstructions).HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.ExecutionNote).HasMaxLength(1000);
        b.Property(x => x.ExecutedAt).HasColumnType("datetimeoffset");
    }
}

public sealed class MedicalReportConfiguration : IEntityTypeConfiguration<MedicalReport>
{
    public void Configure(EntityTypeBuilder<MedicalReport> b)
    {
        b.ToTable("MedicalReports");
        b.HasKey(x => x.Id);
        b.Property(x => x.CareEncounterId).IsRequired();
        b.Property(x => x.MedicalRecordId).IsRequired();
        b.Property(x => x.DoctorProviderId).IsRequired();
        b.Property(x => x.Assessment).HasColumnType("nvarchar(max)").IsRequired();
        b.Property(x => x.Findings).HasColumnType("nvarchar(max)");
        b.Property(x => x.Recommendations).HasColumnType("nvarchar(max)");
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasIndex(x => x.CareEncounterId);
    }
}

public sealed class NursingReportConfiguration : IEntityTypeConfiguration<NursingReport>
{
    public void Configure(EntityTypeBuilder<NursingReport> b)
    {
        b.ToTable("NursingReports");
        b.HasKey(x => x.Id);
        b.Property(x => x.CareEncounterId).IsRequired();
        b.Property(x => x.MedicalRecordId).IsRequired();
        b.Property(x => x.NurseProviderId).IsRequired();
        b.Property(x => x.VitalSigns).HasMaxLength(1000);
        b.Property(x => x.ProceduresPerformed).HasColumnType("nvarchar(max)");
        b.Property(x => x.ExecutedPlanItemsSummary).HasColumnType("nvarchar(max)");
        b.Property(x => x.RemainingPlanItemsSummary).HasColumnType("nvarchar(max)");
        b.Property(x => x.ObservationsAndSymptoms).HasColumnType("nvarchar(max)");
        b.Property(x => x.PatientResponse).HasMaxLength(2000);
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasIndex(x => x.CareEncounterId);
    }
}

public sealed class FollowUpRecordConfiguration : IEntityTypeConfiguration<FollowUpRecord>
{
    public void Configure(EntityTypeBuilder<FollowUpRecord> b)
    {
        b.ToTable("FollowUpRecords");
        b.HasKey(x => x.Id);
        b.Property(x => x.MedicalRecordId).IsRequired();
        b.Property(x => x.TreatmentPlanId).IsRequired();
        b.Property(x => x.FollowUpDate).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
    }
}

public sealed class ServiceRatingConfiguration : IEntityTypeConfiguration<ServiceRating>
{
    public void Configure(EntityTypeBuilder<ServiceRating> b)
    {
        b.ToTable("ServiceRatings");
        b.HasKey(x => x.Id);
        b.Property(x => x.CareEncounterId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.ProviderId).IsRequired();
        b.Property(x => x.Score).IsRequired();
        b.Property(x => x.Comment).HasMaxLength(1000);
        b.Property(x => x.SubmittedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasIndex(x => new { x.CareEncounterId, x.PatientId }).IsUnique();
    }
}
