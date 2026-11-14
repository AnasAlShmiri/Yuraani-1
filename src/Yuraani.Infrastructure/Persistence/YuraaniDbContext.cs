using Microsoft.EntityFrameworkCore;
using Yuraani.Application.Common.Abstractions;
using Yuraani.Domain.CareExecution;
using Yuraani.Domain.CareRequests;
using Yuraani.Domain.Identity;
using Yuraani.Domain.Patients;
using Yuraani.Domain.ProfessionalApproval;
using ApprovalEntity = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;

namespace Yuraani.Infrastructure.Persistence;

public sealed class YuraaniDbContext(DbContextOptions<YuraaniDbContext> options) : DbContext(options), IUnitOfWork
{
    // Identity
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<VerificationChallenge> VerificationChallenges => Set<VerificationChallenge>();
    public DbSet<LoginHistory> LoginHistory => Set<LoginHistory>();
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ProfessionalApproval
    public DbSet<CareProvider> CareProviders => Set<CareProvider>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Nurse> Nurses => Set<Nurse>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<ApprovalEntity> ProfessionalApprovals => Set<ApprovalEntity>();
    public DbSet<ProfessionalApprovalReview> ProfessionalApprovalReviews => Set<ProfessionalApprovalReview>();
    public DbSet<ProfessionalLicense> ProfessionalLicenses => Set<ProfessionalLicense>();

    // Patients
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<LabTestResult> LabTestResults => Set<LabTestResult>();

    // CareRequests
    public DbSet<CareRequest> CareRequests => Set<CareRequest>();
    public DbSet<RequestAssignment> RequestAssignments => Set<RequestAssignment>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // CareExecution
    public DbSet<CareEncounter> CareEncounters => Set<CareEncounter>();
    public DbSet<HomeVisit> HomeVisits => Set<HomeVisit>();
    public DbSet<VideoConsultation> VideoConsultations => Set<VideoConsultation>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentPlanItem> TreatmentPlanItems => Set<TreatmentPlanItem>();
    public DbSet<MedicalReport> MedicalReports => Set<MedicalReport>();
    public DbSet<NursingReport> NursingReports => Set<NursingReport>();
    public DbSet<FollowUpRecord> FollowUpRecords => Set<FollowUpRecord>();
    public DbSet<ServiceRating> ServiceRatings => Set<ServiceRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(YuraaniDbContext).Assembly);
    }
}
