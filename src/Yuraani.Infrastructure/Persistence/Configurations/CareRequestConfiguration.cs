using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.CareRequests;

namespace Yuraani.Infrastructure.Persistence.Configurations;

public sealed class CareRequestConfiguration : IEntityTypeConfiguration<CareRequest>
{
    public void Configure(EntityTypeBuilder<CareRequest> b)
    {
        b.ToTable("CareRequests");
        b.HasKey(x => x.Id);
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.ServiceType).HasConversion<int>().IsRequired();
        b.Property(x => x.SymptomDescription).HasColumnType("nvarchar(2000)").IsRequired();
        b.Property(x => x.InputChannel).HasConversion<int>().IsRequired();
        b.Property(x => x.SuggestedSpecialty).HasMaxLength(200);
        b.Property(x => x.RequestStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.VisitAddress).HasMaxLength(500);
        b.Property(x => x.PreferredDate).HasColumnType("datetimeoffset");
        b.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        b.HasMany(x => x.Assignments)
            .WithOne()
            .HasForeignKey(x => x.CareRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.PatientId);
        b.HasIndex(x => x.RequestStatus);
    }
}

public sealed class RequestAssignmentConfiguration : IEntityTypeConfiguration<RequestAssignment>
{
    public void Configure(EntityTypeBuilder<RequestAssignment> b)
    {
        b.ToTable("RequestAssignments");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProviderId).IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.AssignedAt).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.RespondedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.DeclineReason).HasMaxLength(500);
    }
}

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.ToTable("Appointments");
        b.HasKey(x => x.Id);
        b.Property(x => x.CareRequestId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.ProviderId).IsRequired();
        b.Property(x => x.ScheduledStart).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.ScheduledEnd).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.AppointmentStatus).HasConversion<int>().IsRequired();
        b.Property(x => x.ConfirmedAt).HasColumnType("datetimeoffset");
        b.Property(x => x.CancellationReason).HasMaxLength(500);
        b.HasIndex(x => x.CareRequestId);
        b.HasIndex(x => new { x.PatientId, x.ScheduledStart });
    }
}
